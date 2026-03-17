using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Remote.Leagues;
using Worms.Cli.Resources.Remote.Schemes;

namespace Worms.Cli.League;

internal sealed class LeagueUpdater(
    IRemoteLeagueRetriever remoteLeagueRetriever,
    IRemoteSchemeDownloader remoteSchemeDownloader,
    IWormsArmageddon wormsArmageddon,
    ILogger<LeagueUpdater> logger)
{
    public async Task Update(string leagueName)
    {
        _ = Activity.Current?.SetTag(Telemetry.Spans.League.Id, leagueName);

        var latestVersion = await remoteLeagueRetriever.Retrieve(leagueName);
        var schemesFolder = wormsArmageddon.FindInstallation().SchemesFolder;
        var version = latestVersion.Version.ToString(3);
        var downloadFileName = $"{leagueName}.{version}.wsc";

        logger.LogInformation("Downloading Scheme: {FileName}", downloadFileName);
        _ = Activity.Current?.SetTag(Telemetry.Spans.Scheme.Id, leagueName);
        _ = Activity.Current?.SetTag(Telemetry.Spans.Scheme.SchemeVersion, version);

        await remoteSchemeDownloader.Download(leagueName, downloadFileName, schemesFolder);
    }
}
