using System.Net;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Resources.Remote.Replays;

internal sealed class RemoteReplayCreator(IWormsServerApi api, ILogger<RemoteReplayCreator> logger)
    : IResourceCreator<RemoteReplay, RemoteReplayCreateParameters>
{
    public async Task<RemoteReplay> Create(RemoteReplayCreateParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            var apiReplay = await api.CreateReplay(new CreateReplayDtoV1(parameters.Name, parameters.FilePath))
                .ConfigureAwait(false);
            return new RemoteReplay(apiReplay.Id, apiReplay.Name, apiReplay.Status);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogWarning("You don't have access to the Worms Hub. Please run worms auth or contact an admin");
            }
            else
            {
                logger.LogError(e, "An error occured calling the Worms Hub API");
            }

            return new RemoteReplay("", "", "");
        }
    }
}
