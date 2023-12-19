namespace Worms.Cli.Resources.Remote.Schemes;

internal sealed class RemoteSchemeRetriever(IWormsServerApi api) : IRemoteSchemeRetriever
{
    public async Task<RemoteScheme> Retrieve(string id)
    {
        var apiResult = await api.GetScheme(id).ConfigureAwait(false);
        return new RemoteScheme(apiResult.Name, apiResult.Version);
    }
}
