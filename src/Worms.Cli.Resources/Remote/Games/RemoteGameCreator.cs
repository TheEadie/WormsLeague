using System.Net;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Resources.Remote.Games;

internal sealed class RemoteGameCreator(IWormsServerApi api, ILogger<RemoteGameCreator> logger)
    : IResourceCreator<RemoteGame, string>
{
    public async Task<RemoteGame> Create(string parameters, CancellationToken cancellationToken)
    {
        try
        {
            var apiGame = await api.CreateGame(new CreateGameDtoV1(parameters));
            return new RemoteGame(apiGame.Id, apiGame.Status, apiGame.HostMachine);
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

            return new RemoteGame("", "", "");
        }
    }
}
