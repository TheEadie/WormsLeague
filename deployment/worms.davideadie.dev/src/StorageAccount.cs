using Pulumi;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

namespace worms.davideadie.dev;

public static class StorageAccount
{
    public static Storage.StorageAccount Config(ResourceGroup resourceGroup, Config config)
    {
        return new Storage.StorageAccount("storage-account", new()
        {
            AccountName = "wormstest",
            ResourceGroupName = resourceGroup.Name,
            LargeFileSharesState = Storage.LargeFileSharesState.Enabled,
            Kind = Storage.Kind.StorageV2,

            Sku = new Storage.Inputs.SkuArgs
            {
                Name = Storage.SkuName.Standard_LRS,
            },
        });
    }
}