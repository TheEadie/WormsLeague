using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
    
    public async Task<IReadOnlyCollection<RemoteGame>> Get(string pattern, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var apiGames = await _api.GetGames();
            return apiGames.Select(x=>new RemoteGame(x.Id, x.Status, x.HostMachine)).ToList();
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.Unauthorized)
                logger.Warning("You don't have access to the Worms League Server. Please run worms auth or contact an admin");

            return new List<RemoteGame>(0);
        }
    }
}