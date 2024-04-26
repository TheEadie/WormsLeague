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
    public async Task DownloadLatestUpdate()
    {
        logger.LogDebug("Starting update");

        var cliInfo = cliInfoRetriever.Get();
        logger.LogDebug("{Info}", cliInfo.ToString());

        var latestCliVersion = await cliUpdateRetriever.GetLatestCliVersion().ConfigureAwait(false);
        logger.LogDebug("Latest version: {Version}", latestCliVersion);

        if (cliInfo.Version >= latestCliVersion)
        {
            logger.LogInformation("Worms CLI is up to date");
            return;
        }

        logger.LogInformation("Downloading Worms CLI {Version}", latestCliVersion);

        var updateFolder = fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "Worms",
            ".update");

        EnsureFolderExistsAndIsEmpty(updateFolder);

        await cliUpdateDownloader.DownloadLatestCli(updateFolder).ConfigureAwait(false);
        logger.LogWarning("Update available - To install the update run Install-WormsCli");
    }

    private void EnsureFolderExistsAndIsEmpty(string updateFolder)
    {
        if (fileSystem.Directory.Exists(updateFolder))
        {
            fileSystem.Directory.Delete(updateFolder, true);
        }

        _ = fileSystem.Directory.CreateDirectory(updateFolder);
    }
}
