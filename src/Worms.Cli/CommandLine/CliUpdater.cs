using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.CommandLine;

internal sealed class CliUpdater(
    CliInfoRetriever cliInfoRetriever,
    ICliUpdateRetriever cliUpdateRetriever,
    ICliUpdateDownloader cliUpdateDownloader,
    IFileSystem fileSystem,
    ILogger<CliUpdater> logger)
{
    public async Task DownloadAndInstall(bool force)
    {
        logger.LogDebug("Starting update...");

        var cliInfo = cliInfoRetriever.Get();
        logger.LogDebug("Current Version: {Info}", cliInfo.ToString());

        var latestCliVersion = await cliUpdateRetriever.GetLatestCliVersion().ConfigureAwait(false);
        logger.LogDebug("Latest version: {Version}", latestCliVersion);

        if (cliInfo.Version >= latestCliVersion && !force)
        {
            logger.LogInformation("Worms CLI is up to date");
            return;
        }

        var updateFolder = fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "Worms",
            ".update");

        await DownloadUpdate(latestCliVersion, updateFolder).ConfigureAwait(false);

        InstallUpdate(cliInfo, updateFolder);

        logger.LogInformation("Update complete");
    }

    private async Task DownloadUpdate(Version latestCliVersion, string updateFolder)
    {
        logger.LogInformation("Downloading Worms CLI {Version}...", latestCliVersion);

        if (fileSystem.Directory.Exists(updateFolder))
        {
            fileSystem.Directory.Delete(updateFolder, true);
        }

        _ = fileSystem.Directory.CreateDirectory(updateFolder);
        await cliUpdateDownloader.DownloadLatestCli(updateFolder).ConfigureAwait(false);
        logger.LogInformation("Downloading Complete");
    }

    private void InstallUpdate(CliInfo cliInfo, string updateFolder)
    {
        logger.LogInformation("Installing...");

        var processFileName = cliInfo.FileName;
        var updatePath = fileSystem.Path.Combine(updateFolder, processFileName);
        var installPath = fileSystem.Path.Combine(cliInfo.Folder, processFileName);
        var backupPath = installPath + ".bak";

        logger.LogDebug("Moving {Source} to {Destination}", installPath, backupPath);
        fileSystem.File.Move(installPath, backupPath, true);

        logger.LogDebug("Moving {Source} to {Destination}", updatePath, installPath);
        fileSystem.File.Move(updatePath, installPath);

        foreach (var file in Directory.GetFiles(updateFolder))
        {
            var destination = Path.Combine(cliInfo.Folder, Path.GetFileName(file));
            logger.LogDebug("Copying {Source} to {Destination}", file, destination);
            File.Copy(file, destination, true);
        }
    }
}
