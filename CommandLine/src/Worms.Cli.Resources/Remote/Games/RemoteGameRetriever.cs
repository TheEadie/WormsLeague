using System;
using System.Collections.Generic;
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
        return await _api.Get<IReadOnlyCollection<RemoteGame>>(new Uri("api/v1/games", UriKind.Relative));
    }
}