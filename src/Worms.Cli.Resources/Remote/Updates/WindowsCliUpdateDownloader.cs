using System.IO.Abstractions;
using System.IO.Compression;

namespace Worms.Cli.Resources.Remote.Updates;

internal sealed class WindowsCliUpdateDownloader(IWormsServerApi api, IFileSystem fileSystem) : ICliUpdateDownloader
{
    public async Task DownloadLatestCli(string updateFolder)
    {
        const string downloadFileName = "update.zip";
        var archiveFilePath = Path.Combine(updateFolder, downloadFileName);

        // Download file
        var bytes = await api.DownloadLatestCli("windows");
        await File.WriteAllBytesAsync(archiveFilePath, bytes);

        // Unzip file
        await using var zip = await ZipFile.OpenReadAsync(archiveFilePath);
        await zip.ExtractToDirectoryAsync(updateFolder);
        await zip.DisposeAsync();

        // Delete zip file
        fileSystem.File.Delete(archiveFilePath);
    }
}
