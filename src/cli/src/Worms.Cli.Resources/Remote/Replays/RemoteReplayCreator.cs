using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Worms.Cli.Resources.Remote.Replays;

internal class RemoteReplayCreator : IResourceCreator<RemoteReplay, string>
{
    private readonly IWormsServerApi _api;

    public RemoteReplayCreator(IWormsServerApi api)
    {
        _api = api;
    }

    public async Task<RemoteReplay> Create(string parameters, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var apiReplay = await _api.CreateReplay(parameters);
            return new RemoteReplay(apiReplay.Id);
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

            return new RemoteReplay("");
        }
    }
}