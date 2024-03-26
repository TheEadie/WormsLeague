using System.Net;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Resources.Remote.Games;

internal sealed class RemoteGameUpdater(IWormsServerApi api, ILogger<RemoteGameUpdater> logger) : IRemoteGameUpdater
{
    public async Task SetGameComplete(RemoteGame game, CancellationToken cancellationToken)
    {
        try
        {
            await api.UpdateGame(new GamesDtoV1(game.Id, "Complete", game.HostMachine)).ConfigureAwait(false);
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
        }
    }
}
