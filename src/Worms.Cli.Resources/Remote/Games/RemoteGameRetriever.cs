using System.Net;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Resources.Remote.Games;

internal sealed class RemoteGameRetriever(IWormsServerApi api, ILogger<RemoteGameRetriever> logger)
    : IResourceRetriever<RemoteGame>
{
    public Task<IReadOnlyCollection<RemoteGame>> Retrieve(CancellationToken cancellationToken) =>
        Retrieve("*", cancellationToken);

    public async Task<IReadOnlyCollection<RemoteGame>> Retrieve(string pattern, CancellationToken cancellationToken)
    {
        try
        {
            var apiGames = await api.GetGames().ConfigureAwait(false);
            return apiGames.Select(x => new RemoteGame(x.Id, x.Status, x.HostMachine)).ToList();
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

            return [];
        }
    }
}
