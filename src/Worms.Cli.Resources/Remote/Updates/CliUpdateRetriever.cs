namespace Worms.Cli.Resources.Remote.Updates;

internal sealed class CliUpdateRetriever : ICliUpdateRetriever
{
    private readonly IWormsServerApi _api;

    public CliUpdateRetriever(IWormsServerApi api) => _api = api;

    public async Task<Version> GetLatestCliVersion()
    {
        var apiResult = await _api.GetLatestCliDetails();
        return apiResult.LatestVersion;
    }
}
