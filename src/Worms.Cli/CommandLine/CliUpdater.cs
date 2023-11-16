using System.IO.Abstractions;
using Serilog;
using Worms.Cli.CommandLine.PackageManagers;
using Worms.Cli.Configuration;

namespace Worms.Cli.CommandLine;

internal sealed class CliUpdater(
    CliInfoRetriever cliInfoRetriever,
    IGitHubReleasePackageManagerFactory packageManagerFactory,
    IFileSystem fileSystem)
{
    public async Task DownloadLatestUpdate(Config config, ILogger logger)
    {
        logger.Verbose("Starting update");

        var cliInfo = cliInfoRetriever.Get(logger);
        logger.Verbose(cliInfo.ToString());

        var packageManager = packageManagerFactory.Create(
            "TheEadie",
            "WormsLeague",
            "cli/v",
            config.GitHubPersonalAccessToken);

        var versions = (await packageManager.GetAvailableVersions().ConfigureAwait(false)).ToList();
        logger.Verbose($"Available versions: {string.Join(", ", versions)}");

        var latestVersion = versions.MaxBy(x => x);
        logger.Verbose($"Latest version: {latestVersion}");

        if (latestVersion is null)
        {
            logger.Warning("No versions of Worms CLI available");
            return;
        }

        if (cliInfo.Version > latestVersion)
        {
            logger.Information("Worms CLI is up to date");
            return;
        }

        logger.Information($"Downloading Worms CLI {latestVersion}");

        var updateFolder = fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "Worms",
            ".update");

        EnsureFolderExistsAndIsEmpty(updateFolder);

        await packageManager.DownloadVersion(latestVersion, updateFolder).ConfigureAwait(false);
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
