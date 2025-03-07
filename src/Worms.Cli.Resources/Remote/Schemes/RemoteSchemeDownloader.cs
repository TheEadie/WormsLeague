using System.IO.Abstractions;

namespace Worms.Cli.Resources.Remote.Schemes;

internal sealed class RemoteSchemeDownloader(IWormsServerApi api, IFileSystem fileSystem) : IRemoteSchemeDownloader
{
    public async Task Download(string id, string destinationFilename, string destinationFolder)
    {
        var filePath = Path.Combine(destinationFolder, destinationFilename);

        var bytes = await api.DownloadScheme(id);
        await fileSystem.File.WriteAllBytesAsync(filePath, bytes);
    }
}
