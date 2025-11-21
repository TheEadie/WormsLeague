using Pulumi;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

namespace Worms.Hub.Infrastructure.Azure;

internal static class FileShare
{
    public static Storage.FileShare Config(
        ResourceGroup resourceGroup,
        Storage.StorageAccount storageAccount,
        Config config) =>
        new(
            "file-share",
            new()
            {
                AccountName = storageAccount.Name,
                ResourceGroupName = resourceGroup.Name,
                ShareName = "file-share",
            });
}
