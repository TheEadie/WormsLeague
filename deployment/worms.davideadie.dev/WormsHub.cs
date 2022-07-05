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
                },
            },
            Template = new TemplateArgs
            {
                Containers = {
                    new ContainerArgs
                    {
                        Name = "gateway",
                        Image = "theeadie/worms-server-gateway:0.1.0",
                    }
                },
                Scale = new ScaleArgs
                {
                    MaxReplicas = 1,
                    MinReplicas = 0,
                },
            }
        });

        var server = new Pulumi.AzureNative.DBforPostgreSQL.Server("server", new Pulumi.AzureNative.DBforPostgreSQL.ServerArgs
        {
            ServerName = "worms",
            ResourceGroupName = resourceGroup.Name,
            Location= "eastus",
            Properties = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.ServerPropertiesForDefaultCreateArgs
            {
                AdministratorLogin = config.RequireSecret("database_user"),
                AdministratorLoginPassword = config.RequireSecret("database_password"),
                CreateMode = "Default",
                Version = Pulumi.AzureNative.DBforPostgreSQL.ServerVersion.ServerVersion_11,
            },
            Sku = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.SkuArgs
            {
                Capacity = 1,
                Family = "Gen5",
                Name = "B_Gen5_1",
                Tier = "Basic",
            },
        });

        var database = new  Pulumi.AzureNative.DBforPostgreSQL.Database("database", new  Pulumi.AzureNative.DBforPostgreSQL.DatabaseArgs
        {
            DatabaseName = "worms",
            ResourceGroupName = resourceGroup.Name,
            Charset = "UTF8",
            ServerName = server.Name,
        });

        Url = Output.Format($"https://{containerApp.Configuration.Apply(c => c.Ingress).Apply(i => i.Fqdn)}");
    }

    [Output("url")] Output<string> Url { get; set; }

}
