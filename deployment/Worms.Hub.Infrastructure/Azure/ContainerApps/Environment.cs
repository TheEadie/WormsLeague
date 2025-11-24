using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

namespace Worms.Hub.Infrastructure.Azure.ContainerApps;

internal static class Environment
{
    public static (ManagedEnvironment, ManagedEnvironmentsStorage) Config(
        ResourceGroup resourceGroup,
        Workspace logAnalytics,
        Storage.StorageAccount storageAccount,
        Storage.FileShare fileShare)
    {
        var logAnalyticsSharedKeys = GetSharedKeys.Invoke(
            new()
            {
                ResourceGroupName = resourceGroup.Name,
                WorkspaceName = logAnalytics.Name
            });

        var managedEnvironment = new ManagedEnvironment(
            "azure-container-apps-environment",
            new()
            {
                EnvironmentName = "Worms-Hub",
                ResourceGroupName = resourceGroup.Name,
                AppLogsConfiguration = new AppLogsConfigurationArgs
                {
                    Destination = "log-analytics",
                    LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                    {
                        CustomerId = logAnalytics.CustomerId,
                        SharedKey = logAnalyticsSharedKeys.Apply(
                            x => x.PrimarySharedKey ?? throw new KeyNotFoundException("No primary shared key found")),
                    }
                }
            });

        var managedEnvironmentsStorage = new ManagedEnvironmentsStorage(
            "azure-container-apps-storage",
            new()
            {
                StorageName = "worms-hub-storage",
                ResourceGroupName = resourceGroup.Name,
                EnvironmentName = managedEnvironment.Name,
                Properties = new ManagedEnvironmentStoragePropertiesArgs
                {
                    AzureFile = new AzureFilePropertiesArgs
                    {
                        AccessMode = AccessMode.ReadWrite,
                        AccountKey = Storage.ListStorageAccountKeys.Invoke(
                                new()
                                {
                                    ResourceGroupName = resourceGroup.Name,
                                    AccountName = storageAccount.Name
                                })
                            .Apply(x => x.Keys[0].Value),
                        AccountName = storageAccount.Name,
                        ShareName = fileShare.Name,
                    },
                },
            });

        return (managedEnvironment, managedEnvironmentsStorage);
    }
}
