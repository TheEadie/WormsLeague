using Pulumi;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

namespace Worms.Hub.Infrastructure;

public static class Queue
{
    public static Storage.Queue Config(
        ResourceGroup resourceGroup,
        Storage.StorageAccount storageAccount,
        Config config) =>
        new("queue", new()
    {
        AccountName = storageAccount.Name,
        ResourceGroupName = resourceGroup.Name,
        QueueName = "worms-queue",
    });
}
