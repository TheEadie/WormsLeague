using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Worms.Cli.Resources.Remote.Games;

internal class RemoteGameRetriever : IResourceRetriever<RemoteGame>
{
    private readonly IWormsServerApi _api;

    public RemoteGameRetriever(IWormsServerApi api)
    {
        _api = api;
    }
    
    public async Task<IReadOnlyCollection<RemoteGame>> Get(string pattern = "*")
    {
        var apiGames = await _api.GetGames();
        return apiGames.Select(x=>new RemoteGame(x.Id, x.Status, x.HostMachine)).ToList();
    }
}