﻿using System.Net;
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
            var apiGame = await _api.CreateGame(parameters);
            return new RemoteGame(apiGame.Id, apiGame.Status, apiGame.HostMachine);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.Unauthorized)
                logger.Warning("You don't have access to the Worms League Server. Please run worms auth or contact an admin");

            return new RemoteGame("", "", "");
        }
    }
}