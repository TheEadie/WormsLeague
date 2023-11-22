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
        var bytes = await api.DownloadLatestCli("windows").ConfigureAwait(false);
        await File.WriteAllBytesAsync(archiveFilePath, bytes).ConfigureAwait(false);

        // Unzip file
        using var zip = ZipFile.OpenRead(archiveFilePath);
        zip.ExtractToDirectory(updateFolder);
        zip.Dispose();

        // Delete zip file
        fileSystem.File.Delete(archiveFilePath);
    }
}
