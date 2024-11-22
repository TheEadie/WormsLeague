using Pulumi;
using Pulumi.AzureNative.Resources;
using Storage = Pulumi.AzureNative.Storage;

namespace Worms.Hub.Infrastructure;

public static class StorageAccount
{
    public static (Storage.StorageAccount storageAccount, Output<string> accessToken) Config(ResourceGroup resourceGroup, Config config)
    {
        var storage = new Storage.StorageAccount(
            "storage-account",
            new()
            {
                AccountName = Utils.GetResourceNameAlphaNumericOnly("wormstest"),
                ResourceGroupName = resourceGroup.Name,
                LargeFileSharesState = Storage.LargeFileSharesState.Enabled,
                Kind = Storage.Kind.StorageV2,
                Sku = new Storage.Inputs.SkuArgs
                {
                    Name = Storage.SkuName.Standard_LRS,
                },
            });
        var accessKey = Output.Tuple(resourceGroup.Name, storage.Name)
            .Apply(
                async x => (await Storage.ListStorageAccountKeys.InvokeAsync(
                    new Storage.ListStorageAccountKeysArgs {AccountName = x.Item2, ResourceGroupName = x.Item1})).Keys[0].Value);
        return (storage, accessKey);
    }

}
