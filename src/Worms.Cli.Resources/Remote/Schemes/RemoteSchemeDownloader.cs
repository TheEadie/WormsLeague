using System.IO.Abstractions;

namespace Worms.Cli.Resources.Remote.Schemes;

internal sealed class RemoteSchemeDownloader(IWormsServerApi api, IFileSystem fileSystem) : IRemoteSchemeDownloader
{
    public async Task Download(string id, string destinationFolder)
    {
        var downloadFileName = $"{id}.wsc";
        var filePath = Path.Combine(destinationFolder, downloadFileName);

        var bytes = await api.DownloadScheme(id).ConfigureAwait(false);
        await fileSystem.File.WriteAllBytesAsync(filePath, bytes).ConfigureAwait(false);
    }
}
