using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine.PackageManagers;
using Worms.Cli.Configuration;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater
{
    private readonly IGitHubReleasePackageManagerFactory _packageManagerFactory;
    private readonly IWormsLocator _wormsLocator;

    public LeagueUpdater(IGitHubReleasePackageManagerFactory packageManagerFactory, IWormsLocator wormsLocator)
    {
        _packageManagerFactory = packageManagerFactory;
        _wormsLocator = wormsLocator;
    }

    public async Task Update(Config config, ILogger logger)
    {
        var packageManager = _packageManagerFactory.Create(
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

        var schemesFolder = _wormsLocator.Find().SchemesFolder;

        await packageManager.DownloadVersion(latestVersion, schemesFolder).ConfigureAwait(false);
    }
}
