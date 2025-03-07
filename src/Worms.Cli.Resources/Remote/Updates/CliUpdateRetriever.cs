namespace Worms.Cli.Resources.Remote.Updates;

internal sealed class CliUpdateRetriever(IWormsServerApi api) : ICliUpdateRetriever
{
    public async Task<Version> GetLatestCliVersion()
    {
        var apiResult = await api.GetLatestCliDetails();
        return apiResult.LatestVersion;
    }
}
