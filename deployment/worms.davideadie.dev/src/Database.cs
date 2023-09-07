using Pulumi;
using Pulumi.AzureNative.Resources;
using DBForPostgreSQL = Pulumi.AzureNative.DBforPostgreSQL;

namespace worms.davideadie.dev;

public static class Database
{
    public static (DBForPostgreSQL.Server, DBForPostgreSQL.Database) Config(ResourceGroup resourceGroup, Config config)
    {
        var server = new DBForPostgreSQL.Server("postgres-server", new()
        {
            ServerName = Utils.GetResourceName("worms"),
            ResourceGroupName = resourceGroup.Name,
            Version = DBForPostgreSQL.ServerVersion.ServerVersion_14,
            AdministratorLogin = config.RequireSecret("database_user"),
            AdministratorLoginPassword = config.RequireSecret("database_password"),
            CreateMode = "Default",

            Sku = new DBForPostgreSQL.Inputs.SkuArgs
            {
                Name = "Standard_B1ms",
                Tier = "Burstable",
            },

            Storage = new DBForPostgreSQL.Inputs.StorageArgs
            {
                StorageSizeGB = 32,
            },

            Backup = new DBForPostgreSQL.Inputs.BackupArgs
            {
                BackupRetentionDays = 7,
            }
        });

        var database = new DBForPostgreSQL.Database("postgres-database", new()
        {
            DatabaseName = "worms",
            ResourceGroupName = resourceGroup.Name,
            ServerName = server.Name,
        });

        var sqlFwRuleAllowAll = new DBForPostgreSQL.FirewallRule("postgres-firewall-rule", new()
        {
            EndIpAddress = "0.0.0.0",
            FirewallRuleName = "AllowAllWindowsAzureIps",
            ResourceGroupName = resourceGroup.Name,
            ServerName = server.Name,
            StartIpAddress = "0.0.0.0",
        });
        
        return (server, database);
    }
}