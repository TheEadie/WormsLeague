using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

internal sealed class RemoteGameCreator(IWormsServerApi api) : IResourceCreator<RemoteGame, string>
{
    public async Task<RemoteGame> Create(string parameters, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var apiGame = await api.CreateGame(new CreateGameDtoV1(parameters)).ConfigureAwait(false);
            return new RemoteGame(apiGame.Id, apiGame.Status, apiGame.HostMachine);
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

            return new RemoteGame("", "", "");
        }
    }
}
