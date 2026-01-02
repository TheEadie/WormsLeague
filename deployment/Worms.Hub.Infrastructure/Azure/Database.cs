using Pulumi;
using Pulumi.AzureNative.Resources;
using DBForPostgreSQL = Pulumi.AzureNative.DBforPostgreSQL;

namespace Worms.Hub.Infrastructure.Azure;

internal static class Database
{
    public static (DBForPostgreSQL.Server, DBForPostgreSQL.Database, Output<string>, Output<string> Version) Config(
        ResourceGroup resourceGroup,
        Config config)
    {
        var version = Output<string>.Create(Task.FromResult(config.Require("database-version")));

        var randomPassword = new Pulumi.Random.RandomPassword(
            "database-password",
            new()
            {
                Length = 32,
                Special = true,
            });

        var server = new DBForPostgreSQL.Server(
            "postgres-server",
            new()
            {
                ServerName = Utils.GetResourceName("worms"),
                ResourceGroupName = resourceGroup.Name,
                Version = "17",
                AdministratorLogin = "worms_user",
                AdministratorLoginPassword = randomPassword.Result,
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

        var database = new DBForPostgreSQL.Database(
            "postgres-database",
            new()
            {
                DatabaseName = "worms",
                ResourceGroupName = resourceGroup.Name,
                ServerName = server.Name,
            });

        var sqlFwRuleAllowAll = new DBForPostgreSQL.FirewallRule(
            "postgres-firewall-rule",
            new()
            {
                EndIpAddress = "0.0.0.0",
                FirewallRuleName = "AllowAllWindowsAzureIps",
                ResourceGroupName = resourceGroup.Name,
                ServerName = server.Name,
                StartIpAddress = "0.0.0.0",
            });

        if (Pulumi.Deployment.Instance.StackName != "prod")
        {
            var myIp = Output.Create(Utils.GetMyPublicIp());

            var firewallRule = new DBForPostgreSQL.FirewallRule(
                "allow-my-ip",
                new()
                {
                    ResourceGroupName = resourceGroup.Name,
                    ServerName = server.Name,
                    StartIpAddress = myIp,
                    EndIpAddress = myIp
                });
        }

        return (server, database, randomPassword.Result, version);
    }
}
