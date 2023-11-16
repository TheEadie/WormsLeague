using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine.PackageManagers;
using Worms.Cli.Configuration;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater(
    IGitHubReleasePackageManagerFactory packageManagerFactory,
    IWormsLocator wormsLocator)
{
    public async Task Update(Config config, ILogger logger)
    {
        var packageManager = packageManagerFactory.Create(
            "TheEadie",
            "WormsLeague",
            "schemes/v",
            config.GitHubPersonalAccessToken);

        var versions = (await packageManager.GetAvailableVersions().ConfigureAwait(false)).ToList();
        logger.Verbose($"Available versions: {string.Join(", ", versions)}");

        var latestVersion = versions.MaxBy(x => x);
        logger.Verbose($"Latest version: {latestVersion}");

        if (latestVersion is null)
        {
            logger.Warning("No versions of Schemes are available");
            return;
        }

        logger.Information($"Downloading Schemes: {latestVersion}");

        var schemesFolder = wormsLocator.Find().SchemesFolder;

        await packageManager.DownloadVersion(latestVersion, schemesFolder).ConfigureAwait(false);
    }
}
