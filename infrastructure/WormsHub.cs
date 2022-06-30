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

        Url = Output.Format($"https://{containerApp.Configuration.Apply(c => c.Ingress).Apply(i => i.Fqdn)}");
    }

    [Output("url")] Output<string> Url { get; set; }

}
