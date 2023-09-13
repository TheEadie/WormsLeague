using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

internal class RemoteGameUpdater : IRemoteGameUpdater
{
    private readonly IWormsServerApi _api;

    public RemoteGameUpdater(IWormsServerApi api)
    {
        _api = api;
    }

    public async Task SetGameComplete(RemoteGame game, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            await _api.UpdateGame(new WormsServerApi.GamesDtoV1(game.Id, "Complete", game.HostMachine));
        }
        catch (HttpRequestException e)
        {
            switch (e.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    logger.Warning(
                        "You don't have access to the Worms Hub. Please run worms auth or contact an admin");
                    break;
                default:
                    logger.Error(e, "An error occured calling the Worms Hub API");
                    break;
            }
        }
    }
}