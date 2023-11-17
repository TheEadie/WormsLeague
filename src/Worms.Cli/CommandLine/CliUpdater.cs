using System.IO.Abstractions;
using Serilog;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.CommandLine;

internal sealed class CliUpdater(
    CliInfoRetriever cliInfoRetriever,
    ICliUpdateRetriever cliUpdateRetriever,
    ICliUpdateDownloader cliUpdateDownloader,
    IFileSystem fileSystem)
{
    public async Task DownloadLatestUpdate(ILogger logger)
    {
        logger.Verbose("Starting update");

        var cliInfo = cliInfoRetriever.Get(logger);
        logger.Verbose(cliInfo.ToString());

        var latestCliVersion = await cliUpdateRetriever.GetLatestCliVersion();
        logger.Verbose($"Latest version: {latestCliVersion}");

        if (cliInfo.Version >= latestCliVersion)
        {
            logger.Information("Worms CLI is up to date");
            return;
        }

        logger.Information($"Downloading Worms CLI {latestCliVersion}");

        var updateFolder = fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "Worms",
            ".update");

        EnsureFolderExistsAndIsEmpty(updateFolder);

        await cliUpdateDownloader.DownloadLatestCli(updateFolder).ConfigureAwait(false);
        logger.Warning("Update available - To install the update run Install-WormsCli");
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
