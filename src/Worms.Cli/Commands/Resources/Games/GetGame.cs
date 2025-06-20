using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Remote.Games;

namespace Worms.Cli.Commands.Resources.Games;

internal sealed class GetGame : Command
{
    public static readonly Argument<string> GameName = new("name")
    {
        Description =
            "Optional: The name or search pattern for the Game to be retrieved. Wildcards (*) are supported",
        DefaultValueFactory = _ => ""
    };

    public GetGame()
        : base("game", "Retrieves information for current games")
    {
        Aliases.Add("games");
        Arguments.Add(GameName);
    }
}

internal sealed class GetGameHandler(ResourceGetter<RemoteGame> gameRetriever, ILogger<GetGameHandler> logger)
    : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Game.SpanNameGet);
        var name = parseResult.GetRequiredValue(GetGame.GameName);
        var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;

        var games = await gameRetriever.GetResources(name, cancellationToken);

        if (!games.IsValid)
        {
            games.LogErrors(logger);
            return 1;
        }

        gameRetriever.PrintResources(games.Value, Console.Out, windowWidth);
        return 0;
    }
}
