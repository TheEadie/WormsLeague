using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;
using Worms.Hub.Infrastructure.ContainerApps;

namespace Worms.Hub.Infrastructure;

public class WormsHub : Stack
{
    [Output("api-url")]
    public Output<string> ApiUrl { get; set; }

    [Output("database-jdbc")]
    public Output<string> DatabaseJdbc { get; set; }

    [Output("database-adonet")]
    public Output<string> DatabaseAdoNet { get; set; }

    [Output("database-user")]
    public Output<string?> DatabaseUser { get; set; }

    [Output("database-password")]
    public Output<string> DatabasePassword { get; set; }

    public WormsHub()
    {
        var config = new Config();

        // Resource Group
        var resourceGroup = new ResourceGroup(
            "resource-group",
            new() { ResourceGroupName = Utils.GetResourceName("Worms-Hub") });

        // Log Analytics space
        var logAnalytics = new Workspace(
            "workspace",
            new()
            {
                WorkspaceName = "Worms-Hub",
                ResourceGroupName = resourceGroup.Name,
            });

        // Storage
        var storage = StorageAccount.Config(resourceGroup, config);
        var fileShare = FileShare.Config(resourceGroup, storage, config);
        var (server, database, databasePassword) = Database.Config(resourceGroup, config);

        DatabaseJdbc = Output.Format(
            $"jdbc:postgresql://{server.FullyQualifiedDomainName}/{database.Name}?user={server.AdministratorLogin}&password={databasePassword}");
        DatabaseAdoNet = Output.Format(
            $"Server={server.FullyQualifiedDomainName};Port=5432;Database={database.Name};User Id={server.AdministratorLogin};Password={databasePassword}");
        DatabaseUser = server.AdministratorLogin;
        DatabasePassword = databasePassword;

        // Containers
        var (containerApp, containerAppStorage) = Environment.Config(resourceGroup, logAnalytics, storage, fileShare);

        // Gateway
        Dns.Config(config, containerApp);
        var gateway = Task.Run(() => Gateway.Config(resourceGroup, config, containerApp, containerAppStorage, DatabaseAdoNet)).GetAwaiter().GetResult();

        ApiUrl = Output.Format($"https://{gateway.Configuration.Apply(c => c?.Ingress).Apply(i => i?.Fqdn)}");
    }
}
