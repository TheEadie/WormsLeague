using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine.PackageManagers;
using Worms.Cli.Configuration;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater
{
    private readonly GitHubReleasePackageManager _packageManager;
    private readonly IWormsLocator _wormsLocator;

    public LeagueUpdater(GitHubReleasePackageManager packageManager, IWormsLocator wormsLocator)
    {
        _packageManager = packageManager;
        _wormsLocator = wormsLocator;
    }

    public async Task Update(Config config, ILogger logger)
    {
        _packageManager.Connect("TheEadie", "WormsLeague", "schemes/v", config.GitHubPersonalAccessToken);

        var versions = (await _packageManager.GetAvailableVersions().ConfigureAwait(false)).ToList();
        logger.Verbose($"Available versions: {string.Join(", ", versions)}");

        var latestVersion = versions.OrderByDescending(x => x).FirstOrDefault();
        logger.Verbose($"Latest version: {latestVersion}");

        logger.Information($"Downloading Schemes: {latestVersion}");

        var schemesFolder = _wormsLocator.Find().SchemesFolder;

        await _packageManager.DownloadVersion(latestVersion, schemesFolder).ConfigureAwait(false);
    }
}
