using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

internal class RemoteGameCreator : IResourceCreator<RemoteGame, string>
{
    private readonly IWormsServerApi _api;

    public RemoteGameCreator(IWormsServerApi api)
    {
        _api = api;
    }

    public async Task<RemoteGame> Create(string parameters, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var apiGame = await _api.CreateGame(new WormsServerApi.CreateGameDtoV1(parameters));
            return new RemoteGame(apiGame.Id, apiGame.Status, apiGame.HostMachine);
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

            return new RemoteGame("", "", "");
        }
    }
}