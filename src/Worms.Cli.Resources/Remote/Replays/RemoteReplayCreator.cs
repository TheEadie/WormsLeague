using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Replays;

internal sealed class RemoteReplayCreator(IWormsServerApi api)
    : IResourceCreator<RemoteReplay, RemoteReplayCreateParameters>
{
    public async Task<RemoteReplay> Create(
        RemoteReplayCreateParameters parameters,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiReplay = await api.CreateReplay(new CreateReplayDtoV1(parameters.Name, parameters.FilePath));
            return new RemoteReplay(apiReplay.Id, apiReplay.Name, apiReplay.Status);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.Warning("You don't have access to the Worms Hub. Please run worms auth or contact an admin");
            }
            else
            {
                logger.Error(e, "An error occured calling the Worms Hub API");
            }

            return new RemoteReplay("", "", "");
        }
    }
}
