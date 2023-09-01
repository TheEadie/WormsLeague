using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;

namespace worms.davideadie.dev;

public class WormsHub : Stack
{
    public WormsHub()
    {
        var isProd = Pulumi.Deployment.Instance.StackName == "prod";
        var config = new Config();

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("resource-group", new()
        {
            ResourceGroupName = Utils.GetResourceName("Worms-Hub")
        });

        var logAnalytics = new Workspace("workspace", new()
        {
            WorkspaceName = "Worms-Hub",
            ResourceGroupName = resourceGroup.Name,
        });

        var storage = StorageAccount.Config(resourceGroup, config);
        var fileShare = FileShare.Config(resourceGroup, storage, config);
        Database.Config(resourceGroup, config);
        var containerApp = ContainerApps.Config(resourceGroup, config, logAnalytics, storage, fileShare);

        var protocol = isProd ? "https://" : "http://";
        ApiUrl = Output.Format($"{protocol}{containerApp.Configuration.Apply(c => c.Ingress).Apply(i => i.Fqdn)}");
    }

    [Output("url")] 
    public Output<string> ApiUrl { get; set; }

}