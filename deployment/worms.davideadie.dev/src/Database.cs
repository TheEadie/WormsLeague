using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using DBforPostgreSQL = Pulumi.AzureNative.DBforPostgreSQL;

public static class Database
{
    public static void Config(ResourceGroup resourceGroup, Config config)
    {
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
    }
}