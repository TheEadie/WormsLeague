using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Remote.Schemes;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater(
    IRemoteSchemeRetriever remoteSchemeRetriever,
    IRemoteSchemeDownloader remoteSchemeDownloader,
    IWormsLocator wormsLocator)
{
    public async Task Update(string leagueName, ILogger logger)
    {
        var latestVersion = await remoteSchemeRetriever.Retrieve(leagueName).ConfigureAwait(false);
        var schemesFolder = wormsLocator.Find().SchemesFolder;

        logger.Information($"Downloading Schemes: {latestVersion.Version}");
        await remoteSchemeDownloader.Download(leagueName, schemesFolder).ConfigureAwait(false);
    }
}
