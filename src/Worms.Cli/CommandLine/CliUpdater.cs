using System.Diagnostics;
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
        _ = Activity.Current?.SetTag(Telemetry.Attributes.Update.LatestCliVersion, latestCliVersion);

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

        var updateFolderExists = fileSystem.Directory.Exists(updateFolder);
        _ = Activity.Current?.AddTag(Telemetry.Attributes.Update.UpdateFolderExists, updateFolderExists);

        if (updateFolderExists)
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

        var files = Directory.GetFiles(updateFolder);
        _ = Activity.Current?.AddTag(Telemetry.Attributes.Update.NumberOfFiles, files.Length + 1);

        foreach (var file in files)
        {
            var destination = Path.Combine(cliInfo.Folder, Path.GetFileName(file));
            logger.LogDebug("Copying {Source} to {Destination}", file, destination);
            File.Copy(file, destination, true);
        }
    }
}
