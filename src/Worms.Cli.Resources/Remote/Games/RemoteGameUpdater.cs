using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

internal sealed class RemoteGameUpdater(IWormsServerApi api) : IRemoteGameUpdater
{
    public async Task SetGameComplete(RemoteGame game, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            await api.UpdateGame(new GamesDtoV1(game.Id, "Complete", game.HostMachine));
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
        }
    }
}
