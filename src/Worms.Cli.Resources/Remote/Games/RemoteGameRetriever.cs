using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

internal class RemoteGameRetriever : IResourceRetriever<RemoteGame>
{
    private readonly IWormsServerApi _api;

    public RemoteGameRetriever(IWormsServerApi api)
    {
        _api = api;
    }

    public async Task<IReadOnlyCollection<RemoteGame>> Get(ILogger logger, CancellationToken cancellationToken)
        => await Get("*", logger, cancellationToken);

    public async Task<IReadOnlyCollection<RemoteGame>> Get(string pattern, ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiGames = await _api.GetGames();
            return apiGames.Select(x => new RemoteGame(x.Id, x.Status, x.HostMachine)).ToList();
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

            return new List<RemoteGame>(0);
        }
    }
}