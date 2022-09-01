using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;

class WormsHub : Stack
{
    public WormsHub()
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        var config = new Config();

        
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("worms-hub-resource-group", new ResourceGroupArgs
        {
            ResourceGroupName = stack == "prod" ? "Worms-Hub" : $"Worms-Hub-{stack}"
        });

        var logAnalytics = new Workspace("worms-hub-workspace", new WorkspaceArgs
        {
            WorkspaceName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
        });

        var kubeEnv = new ManagedEnvironment("worms-hub-managed-environment", new ManagedEnvironmentArgs
        {
            EnvironmentName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = "log-analytics",
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = logAnalytics.CustomerId,
                    SharedKey = GetSharedKeys.Invoke(new GetSharedKeysInvokeArgs
                        { 
                            ResourceGroupName = resourceGroup.Name,
                            WorkspaceName = logAnalytics.Name
                        }).Apply(x => x.PrimarySharedKey)
                }
            }
        });

        var certificate = GetCertificate.Invoke(new GetCertificateInvokeArgs
        {
            CertificateName = "worms.davideadie.dev",
            EnvironmentName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
        });

        var containerApp = new ContainerApp("worms-hub-gateway", new ContainerAppArgs
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
                        Image = "theeadie/worms-server-gateway:0.2.0",
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

        var server = new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.Server("server", new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.ServerArgs
        {
            ServerName = "worms",
            ResourceGroupName = resourceGroup.Name,
            Version = Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.ServerVersion.ServerVersion_13,
            AdministratorLogin = config.RequireSecret("database_user"),
            AdministratorLoginPassword = config.RequireSecret("database_password"),
            CreateMode = "Default",
            
            Sku = new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.Inputs.SkuArgs
            {
                Name = "Standard_B1ms",
                Tier = "Burstable",
            },

            Storage = new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.Inputs.StorageArgs {
                StorageSizeGB = 32,
            },

            Backup = new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.Inputs.BackupArgs {
                BackupRetentionDays = 7,
            }
        });

        var database = new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.Database("database", new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.DatabaseArgs
        {
            DatabaseName = "worms",
            ResourceGroupName = resourceGroup.Name,
            ServerName = server.Name,
        });

        var sqlFwRuleAllowAll = new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.FirewallRule("sqlFwRuleAllowAll", new Pulumi.AzureNative.DBforPostgreSQL.V20220120Preview.FirewallRuleArgs
        {
            EndIpAddress = "0.0.0.0",
            FirewallRuleName = "AllowAllWindowsAzureIps",
            ResourceGroupName = resourceGroup.Name,
            ServerName = server.Name,
            StartIpAddress = "0.0.0.0",
        });

        Url = Output.Format($"https://{containerApp.Configuration.Apply(c => c.Ingress).Apply(i => i.Fqdn)}");
    }

    [Output("url")] Output<string> Url { get; set; }

}
