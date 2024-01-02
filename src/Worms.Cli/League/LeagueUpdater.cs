using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Remote.Leagues;
using Worms.Cli.Resources.Remote.Schemes;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater(
    IRemoteLeagueRetriever remoteLeagueRetriever,
    IRemoteSchemeDownloader remoteSchemeDownloader,
    IWormsLocator wormsLocator)
{
    public async Task Update(string leagueName, ILogger logger)
    {
        var latestVersion = await remoteLeagueRetriever.Retrieve(leagueName).ConfigureAwait(false);
        var schemesFolder = wormsLocator.Find().SchemesFolder;

        logger.Information($"Downloading Schemes: {latestVersion.Version}");
        await remoteSchemeDownloader.Download(leagueName, schemesFolder).ConfigureAwait(false);
    }
}
