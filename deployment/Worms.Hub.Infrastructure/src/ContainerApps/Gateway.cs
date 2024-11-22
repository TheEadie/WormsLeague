using System;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.Command.Local;
using CustomDomainArgs = Pulumi.AzureNative.App.Inputs.CustomDomainArgs;

namespace Worms.Hub.Infrastructure.ContainerApps;

public static class Gateway
{
    public static async Task<ContainerApp> Config(
        ResourceGroup resourceGroup,
        Config config,
        ManagedEnvironment managedEnvironment,
        ManagedEnvironmentsStorage managedEnvironmentStorage,
        Output<string> databaseConnectionString,
        Output<string> queueConnectionString)
    {
        var image = config.Require("gateway-image");
        var subdomain = config.Require("subdomain");
        var domain = config.Require("domain");
        var url = $"{subdomain}.{domain}";
        var certificateId = await GetManagedCertificateId();

        var containerApp = new ContainerApp(
            "worms-hub-gateway",
            new()
            {
                ContainerAppName = "worms-gateway",
                ResourceGroupName = resourceGroup.Name,
                ManagedEnvironmentId = managedEnvironment.Id,
                Configuration = new ConfigurationArgs
                {
                    Ingress = new IngressArgs
                    {
                        External = true,
                        TargetPort = 8080,
                        CustomDomains = certificateId is not null ?
                        [
                            new CustomDomainArgs
                            {
                                BindingType = "SniEnabled",
                                CertificateId = certificateId,
                                Name = url,
                            }
                        ] :
                        [
                            new CustomDomainArgs
                            {
                                Name = url,
                                BindingType = "Disabled",
                            }
                        ]
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
                            Name = "queue-connection",
                            Value = queueConnectionString,
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
                            Image = image,
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
                                    Name = "WORMS_CONNECTIONSTRINGS__STORAGE",
                                    SecretRef = "queue-connection",
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
                            StorageName = managedEnvironmentStorage.Name,
                        }
                    }
                }
            });

        // Create a managed certificate - Must be done after env has custom domain added
        var managedCert = new ManagedCertificate(
            "worms-hub-certificate",
            new()
            {
                ManagedCertificateName = "worms-hub-certificate",
                ResourceGroupName = resourceGroup.Name,
                EnvironmentName = managedEnvironment.Name,
                Properties = new ManagedCertificatePropertiesArgs()
                {
                    SubjectName = url,
                    DomainControlValidation = "HTTP"
                }
            },
            new CustomResourceOptions { DependsOn = { containerApp } });

        _ = new Command(
            "bind-certificate",
            new CommandArgs
            {
                Create = Output.Format($"az containerapp hostname bind  --resource-group {resourceGroup.Name} --name {containerApp.Name} --hostname {url} --certificate {managedCert.Id}"),
                Delete = Output.Format($"az containerapp hostname delete --resource-group {resourceGroup.Name} --name {containerApp.Name} --hostname {url} --yes")
            },new CustomResourceOptions { DependsOn = { managedCert } });

        return containerApp;
    }

    private static async Task<string?> GetManagedCertificateId()
    {
        // Managed Cert will only exist from second run
        GetManagedCertificateResult? certificate = null;

        try
        {
            certificate = await GetManagedCertificate.InvokeAsync(
                new()
                {
                    ManagedCertificateName = "worms-hub-certificate",
                    EnvironmentName = "Worms-Hub",
                    ResourceGroupName = Utils.GetResourceName("Worms-Hub")
                });
        }
        catch (Exception)
        {
            // Certificate not found
        }

        return certificate?.Id;
    }
}
