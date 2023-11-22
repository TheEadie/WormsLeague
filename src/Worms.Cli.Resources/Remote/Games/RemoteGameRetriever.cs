using System.Net;
using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

internal sealed class RemoteGameRetriever(IWormsServerApi api) : IResourceRetriever<RemoteGame>
{
    public Task<IReadOnlyCollection<RemoteGame>> Retrieve(ILogger logger, CancellationToken cancellationToken) =>
        Retrieve("*", logger, cancellationToken);

    public async Task<IReadOnlyCollection<RemoteGame>> Retrieve(
        string pattern,
        ILogger logger,
        CancellationToken cancellationToken)
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
                logger.Warning("You don't have access to the Worms Hub. Please run worms auth or contact an admin");
            }
            else
            {
                logger.Error(e, "An error occured calling the Worms Hub API");
            }

            return new List<RemoteGame>(0);
        }
    }
}
