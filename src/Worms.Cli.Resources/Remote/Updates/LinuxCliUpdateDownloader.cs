using System.IO.Abstractions;
using System.IO.Compression;

namespace Worms.Cli.Resources.Remote.Updates;

internal sealed class LinuxCliUpdateDownloader(IWormsServerApi api, IFileSystem fileSystem) : ICliUpdateDownloader
{
    public async Task DownloadLatestCli(string updateFolder)
    {
        const string downloadFileName = "update.tar.gz";
        var archiveFilePath = Path.Combine(updateFolder, downloadFileName);

        // Download file
        var bytes = await api.DownloadLatestCli("linux");
        await File.WriteAllBytesAsync(archiveFilePath, bytes);

        // Unzip tar.gz file
        using var zip = ZipFile.OpenRead(archiveFilePath);
        zip.ExtractToDirectory(updateFolder);
        zip.Dispose();

        // Delete tar.gz file
        fileSystem.File.Delete(archiveFilePath);
    }
}
