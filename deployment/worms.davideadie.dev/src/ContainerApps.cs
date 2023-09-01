using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;
using CustomDomainArgs = Pulumi.AzureNative.App.Inputs.CustomDomainArgs;
using GetCertificate = Pulumi.AzureNative.App.GetCertificate;

namespace worms.davideadie.dev;

public static class ContainerApps
{
    public static ContainerApp Config(ResourceGroup resourceGroup, Config config, Workspace logAnalytics)
    {
        var logAnalyticsSharedKeys = GetSharedKeys.Invoke(new()
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = logAnalytics.Name
        });

        var kubeEnv = new ManagedEnvironment("azure-container-apps-environment", new()
        {
            EnvironmentName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = "log-analytics",
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = logAnalytics.CustomerId,
                    SharedKey = logAnalyticsSharedKeys.Apply(x => x.PrimarySharedKey)
                }
            }
        });

        var customDomainArgs = new InputList<CustomDomainArgs>();

        // Get the SSL cert when deploying to prod only
        if (Pulumi.Deployment.Instance.StackName == "prod")
        {
            var certificate = GetCertificate.Invoke(new()
            {
                CertificateName = "worms.davideadie.dev",
                EnvironmentName = "Worms-Hub",
                ResourceGroupName = resourceGroup.Name,
            });
            
            customDomainArgs.Add(new CustomDomainArgs
            {
                BindingType = "SniEnabled",
                CertificateId = certificate.Apply(x => x.Id),
                Name = "worms.davideadie.dev",
            });
        }

        var containerApp = new ContainerApp("worms-hub-gateway", new()
        {
            ContainerAppName = "worms-gateway",
            ResourceGroupName = resourceGroup.Name,
            ManagedEnvironmentId = kubeEnv.Id,
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = 80,
                    CustomDomains = customDomainArgs,
                },
                Secrets = new InputList<SecretArgs>() {
                    new SecretArgs {
                        Name = "database-connection",
                        Value = config.RequireSecret("database_connectionstring"),
                    },
                    new SecretArgs {
                        Name = "slack-hook-url",
                        Value = config.RequireSecret("slack_hook_url"),
                    }
                }
            },
            Template = new TemplateArgs
            {
                Containers = {
                    new ContainerArgs
                    {
                        Name = "gateway",
                        Image = "theeadie/worms-server-gateway:0.3.9",
                        Env = new InputList<EnvironmentVarArgs>
                        {
                            new EnvironmentVarArgs {
                                Name = "WORMS_CONNECTIONSTRINGS__DATABASE",
                                SecretRef = "database-connection",
                            },
                            new EnvironmentVarArgs {
                                Name = "WORMS_SlackWebHookURL",
                                SecretRef = "slack-hook-url",
                            }
                        }
                    }
                },
                Scale = new ScaleArgs
                {
                    MaxReplicas = 1,
                    MinReplicas = 0,
                },
            }
        });

        return containerApp;
    }
}