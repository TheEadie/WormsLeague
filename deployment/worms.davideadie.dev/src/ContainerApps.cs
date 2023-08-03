using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;

public static class ContainerApps
{
    public static ContainerApp Config(ResourceGroup resourceGroup, Config config, Workspace logAnalytics)
    {
        var logAnalyticsSharedKeys = GetSharedKeys.Invoke(new()
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = logAnalytics.Name
        });

        var kubeEnv = new ManagedEnvironment("worms-hub-managed-environment", new()
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

        var certificate = GetCertificate.Invoke(new()
        {
            CertificateName = "worms.davideadie.dev",
            EnvironmentName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
        });

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
                    CustomDomains = new[]
                    {
                        new CustomDomainArgs
                        {
                            BindingType = "SniEnabled",
                            CertificateId = certificate.Apply(x => x.Id),
                            Name = "worms.davideadie.dev",
                        },
                    }
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