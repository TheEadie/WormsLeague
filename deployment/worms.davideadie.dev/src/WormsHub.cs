using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;

namespace worms.davideadie.dev;

public class WormsHub : Stack
{
    [Output("api-url")]
    public Output<string> ApiUrl { get; set; }
    
    [Output("database-jdbc")]
    public Output<string> DatabaseJdbc { get; set; }
    
    [Output("database-adonet")]
    public Output<string> DatabaseAdoNet { get; set; }
    
    [Output("database-user")]
    public Output<string> DatabaseUser { get; set; }
    
    [Output("database-password")]
    public Output<string> DatabasePassword { get; set; }
    
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
        var (server, database, databasePassword) = Database.Config(resourceGroup, config);
        
        DatabaseJdbc = Output.Format($"jdbc:postgresql://{server.FullyQualifiedDomainName}/{database.Name}?user={server.AdministratorLogin}&password={databasePassword}");
        DatabaseAdoNet = Output.Format($"Server={server.FullyQualifiedDomainName};Port=5432;Database={database.Name};User Id={server.AdministratorLogin};Password={databasePassword}");
        DatabaseUser = server.AdministratorLogin;
        DatabasePassword = databasePassword;
        
        var containerApp = ContainerApps.Config(resourceGroup, config, logAnalytics, storage, fileShare, DatabaseAdoNet);

        var protocol = isProd ? "https://" : "http://";
        
        ApiUrl = Output.Format($"{protocol}{containerApp.Configuration.Apply(c => c.Ingress).Apply(i => i.Fqdn)}");
        
    }
}