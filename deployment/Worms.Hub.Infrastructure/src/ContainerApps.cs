using System;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;
using CustomDomainArgs = Pulumi.AzureNative.App.Inputs.CustomDomainArgs;

namespace Worms.Hub.Infrastructure;

public static class ContainerApps
{
    public static async Task<ContainerApp> Config(
        ResourceGroup resourceGroup,
        Config config,
        Workspace logAnalytics,
        Storage.StorageAccount storageAccount,
        Storage.FileShare fileShare,
        Output<string> databaseConnectionString)
    {
        var domainName = config.Get("domain");

        var logAnalyticsSharedKeys = GetSharedKeys.Invoke(
            new()
            {
                ResourceGroupName = resourceGroup.Name,
                WorkspaceName = logAnalytics.Name
            });

        var kubeEnv = new ManagedEnvironment(
            "azure-container-apps-environment",
            new()
            {
                EnvironmentName = "Worms-Hub",
                ResourceGroupName = resourceGroup.Name,
                AppLogsConfiguration = new AppLogsConfigurationArgs
                {
                    Destination = "log-analytics",
                    LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                    {
                        CustomerId = logAnalytics.CustomerId,
                        SharedKey = logAnalyticsSharedKeys.Apply(
                            x => x.PrimarySharedKey ?? throw new Exception("No primary shared key found")),
                    }
                }
            });

        var managedEnvironmentsStorage = new ManagedEnvironmentsStorage(
            "azure-container-apps-storage",
            new()
            {
                StorageName = "worms-hub-storage",
                ResourceGroupName = resourceGroup.Name,
                EnvironmentName = kubeEnv.Name,
                Properties = new ManagedEnvironmentStoragePropertiesArgs
                {
                    AzureFile = new AzureFilePropertiesArgs
                    {
                        AccessMode = AccessMode.ReadWrite,
                        AccountKey = Storage.ListStorageAccountKeys.Invoke(
                                new()
                                {
                                    ResourceGroupName = resourceGroup.Name,
                                    AccountName = storageAccount.Name
                                })
                            .Apply(x => x.Keys[0].Value),
                        AccountName = storageAccount.Name,
                        ShareName = fileShare.Name,
                    },
                },
            });

        var customDomainArgs = new InputList<CustomDomainArgs>();

        // Managed Cert will only exist from second run
        GetCertificateResult? certificate = null;

        try
        {
            certificate = await GetCertificate.InvokeAsync(
                new()
                {
                    CertificateName = "worms-hub-certificate",
                    EnvironmentName = kubeEnv.GetResourceName(),
                    ResourceGroupName = resourceGroup.GetResourceName()
                });
        }
        catch (Exception)
        {
            // Certificate not found
        }


        if (certificate is not null)
        {
            customDomainArgs.Add(
                new CustomDomainArgs
                {
                    BindingType = "SniEnabled",
                    CertificateId = certificate.Id,
                    Name = domainName,
                });
        }
        else
        {
            customDomainArgs.Add(
                new CustomDomainArgs
                {
                    BindingType = "Disabled",
                    Name = domainName,
                });
        }

        var containerApp = new ContainerApp(
            "worms-hub-gateway",
            new()
            {
                ContainerAppName = "worms-gateway",
                ResourceGroupName = resourceGroup.Name,
                ManagedEnvironmentId = kubeEnv.Id,
                Configuration = new ConfigurationArgs
                {
                    Ingress = new IngressArgs
                    {
                        External = true,
                        TargetPort = 8080,
                        CustomDomains = customDomainArgs,
                    },
                    Secrets =
                    [
                        new SecretArgs
                        {
                            Name = "database-connection",
                            Value = databaseConnectionString,
                        },
                        new SecretArgs
                        {
                            Name = "slack-hook-url",
                            Value = config.RequireSecret("slack_hook_url"),
                        }
                    ]
                },
                Template = new TemplateArgs
                {
                    Containers =
                    {
                        new ContainerArgs
                        {
                            Name = "gateway",
                            Image = "theeadie/worms-server-gateway:0.5.21",
                            Env =
                            [
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_STORAGE__TempReplayFolder",
                                    Value = "/storage/temp-replays",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_STORAGE__CliFolder",
                                    Value = "/storage/cli",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_STORAGE__SchemesFolder",
                                    Value = "/storage/schemes",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_CONNECTIONSTRINGS__DATABASE",
                                    SecretRef = "database-connection",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_SlackWebHookURL",
                                    SecretRef = "slack-hook-url",
                                }
                            ],
                            VolumeMounts =
                            {
                                new VolumeMountArgs
                                {
                                    VolumeName = "worms-hub-volume",
                                    MountPath = "/storage",
                                }
                            }
                        }
                    },
                    Scale = new ScaleArgs
                    {
                        MaxReplicas = 1,
                        MinReplicas = 0,
                    },
                    Volumes =
                    {
                        new VolumeArgs
                        {
                            Name = "worms-hub-volume",
                            StorageType = StorageType.AzureFile,
                            StorageName = managedEnvironmentsStorage.Name,
                        }
                    }
                }
            });

        // Create a managed certificate - Must be done after env has custom domain added
        var cert = new ManagedCertificate(
            "worms-hub-certificate",
            new()
            {
                ManagedCertificateName = "worms-hub-certificate",
                ResourceGroupName = resourceGroup.Name,
                EnvironmentName = kubeEnv.Name,
                Properties = new ManagedCertificatePropertiesArgs()
                {
                    SubjectName = domainName,
                    DomainControlValidation = "HTTP"
                }
            });

        return containerApp;
    }
}
