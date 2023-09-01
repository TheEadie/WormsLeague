using Pulumi;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

namespace worms.davideadie.dev;

public static class FileShare
{
    public static Storage.FileShare Config(ResourceGroup resourceGroup, Storage.StorageAccount storageAccount, Config config)
    {
        return new Storage.FileShare("file-share", new()
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            EnabledProtocols = Storage.EnabledProtocols.SMB,
        });
    }
}