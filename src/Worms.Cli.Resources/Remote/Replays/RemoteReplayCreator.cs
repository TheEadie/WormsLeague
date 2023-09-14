using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Replays;

internal class RemoteReplayCreator : IResourceCreator<RemoteReplay, RemoteReplayCreateParameters>
{
    private readonly IWormsServerApi _api;

    public RemoteReplayCreator(IWormsServerApi api) => _api = api;

    public async Task<RemoteReplay> Create(
        RemoteReplayCreateParameters parameters,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiReplay = await _api.CreateReplay(
                new WormsServerApi.CreateReplayDtoV1(parameters.Name, parameters.FilePath));
            return new RemoteReplay(apiReplay.Id, apiReplay.Name, apiReplay.Status);
        }
        catch (HttpRequestException e)
        {
            switch (e.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    logger.Warning("You don't have access to the Worms Hub. Please run worms auth or contact an admin");
                    break;
                default:
                    logger.Error(e, "An error occured calling the Worms Hub API");
                    break;
            }

            return new RemoteReplay("", "", "");
        }
    }
}
