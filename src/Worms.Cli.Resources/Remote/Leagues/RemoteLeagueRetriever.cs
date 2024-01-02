namespace Worms.Cli.Resources.Remote.Leagues;

internal sealed class RemoteLeagueRetriever(IWormsServerApi api) : IRemoteLeagueRetriever
{
    public async Task<RemoteLeague> Retrieve(string id)
    {
        var apiResult = await api.GetLeague(id).ConfigureAwait(false);
        return new RemoteLeague(apiResult.Name, apiResult.Version);
    }
}
