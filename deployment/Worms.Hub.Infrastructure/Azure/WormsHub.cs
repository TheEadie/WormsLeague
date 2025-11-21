using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;
using Worms.Hub.Infrastructure.Azure.ContainerApps;
using Environment = Worms.Hub.Infrastructure.Azure.ContainerApps.Environment;

namespace Worms.Hub.Infrastructure.Azure;

public static class WormsHub
{
    public record Result(
        Output<string> DatabaseJdbc,
        Output<string> DatabaseAdoNet,
        Output<string?> DatabaseUser,
        Output<string> DatabasePassword,
        Output<string> DatabaseVersion,
        Output<string> ApiUrl,
        Output<string> StorageAccountName);

    public static async Task<Result> Create()
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
        var (storage, storageAccessKey) = StorageAccount.Config(resourceGroup, config);
        var fileShare = FileShare.Config(resourceGroup, storage, config);

        var (server, database, databasePassword, databaseVersion) = Database.Config(resourceGroup, config);

        var databaseJdbc = Output.Format($"jdbc:postgresql://{server.FullyQualifiedDomainName}/{database.Name}");
        var databaseAdoNet = Output.Format(
            $"Server={server.FullyQualifiedDomainName};Port=5432;Database={database.Name};User Id={server.AdministratorLogin};Password={databasePassword}");
        var databaseUser = server.AdministratorLogin;

        var queueConnStr = Output.Format(
            $"DefaultEndpointsProtocol=https;AccountName={storage.Name};AccountKey={storageAccessKey}");

        // Containers
        var (containerApp, containerAppStorage) = Environment.Config(resourceGroup, logAnalytics, storage, fileShare);

        // Gateway
        Dns.Config(config, containerApp);
        var gateway = await Gateway.Config(
            resourceGroup,
            config,
            containerApp,
            containerAppStorage,
            databaseAdoNet,
            queueConnStr);

        // WA Runner
        var waRunner = WaRunner.Config(resourceGroup, config, containerApp, containerAppStorage, queueConnStr);

        // Worker
        var worker = Worker.Config(
            resourceGroup,
            config,
            containerApp,
            containerAppStorage,
            databaseAdoNet,
            queueConnStr);

        var apiUrl = Output.Format($"https://{gateway.Configuration.Apply(c => c?.Ingress).Apply(i => i?.Fqdn)}");

        return new Result(
            databaseJdbc,
            databaseAdoNet,
            databaseUser,
            databasePassword,
            databaseVersion,
            apiUrl,
            storage.Name);
    }
}
