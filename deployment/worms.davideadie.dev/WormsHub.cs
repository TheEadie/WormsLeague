using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using DBforPostgreSQL = Pulumi.AzureNative.DBforPostgreSQL;

class WormsHub : Stack
{
    public WormsHub()
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        var config = new Config();


        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("worms-hub-resource-group", new()
        {
            ResourceGroupName = stack == "prod" ? "Worms-Hub" : $"Worms-Hub-{stack}"
        });

        var logAnalytics = new Workspace("worms-hub-workspace", new()
        {
            WorkspaceName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
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
                    SharedKey = GetSharedKeys.Invoke(new GetSharedKeysInvokeArgs
                    {
                        ResourceGroupName = resourceGroup.Name,
                        WorkspaceName = logAnalytics.Name
                    }).Apply(x => x.PrimarySharedKey)
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

        var server = new DBforPostgreSQL.Server("server", new()
        {
            ServerName = "worms",
            ResourceGroupName = resourceGroup.Name,
            Version = DBforPostgreSQL.ServerVersion.ServerVersion_14,
            AdministratorLogin = config.RequireSecret("database_user"),
            AdministratorLoginPassword = config.RequireSecret("database_password"),
            CreateMode = "Default",

            Sku = new DBforPostgreSQL.Inputs.SkuArgs
            {
                Name = "Standard_B1ms",
                Tier = "Burstable",
            },

            Storage = new DBforPostgreSQL.Inputs.StorageArgs
            {
                StorageSizeGB = 32,
            },

            Backup = new DBforPostgreSQL.Inputs.BackupArgs
            {
                BackupRetentionDays = 7,
            }
        });

        var database = new DBforPostgreSQL.Database("database", new()
        {
            DatabaseName = "worms",
            ResourceGroupName = resourceGroup.Name,
            ServerName = server.Name,
        });

        var sqlFwRuleAllowAll = new DBforPostgreSQL.FirewallRule("sqlFwRuleAllowAll", new()
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
