using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Remote.Leagues;
using Worms.Cli.Resources.Remote.Schemes;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater(
    IRemoteLeagueRetriever remoteLeagueRetriever,
    IRemoteSchemeDownloader remoteSchemeDownloader,
    IWormsLocator wormsLocator,
    ILogger<LeagueUpdater> logger)
{
    public async Task Update(string leagueName)
    {
        var latestVersion = await remoteLeagueRetriever.Retrieve(leagueName).ConfigureAwait(false);
        var schemesFolder = wormsLocator.Find().SchemesFolder;
        var downloadFileName = $"{leagueName}.{latestVersion.Version.ToString(3)}.wsc";

        logger.LogInformation("Downloading Scheme: {FileName}", downloadFileName);
        await remoteSchemeDownloader.Download(leagueName, downloadFileName, schemesFolder).ConfigureAwait(false);
    }
}
