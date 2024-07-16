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
    public static readonly Argument<string> GameName = new(
        "name",
        () => "",
        "Optional: The name or search pattern for the Game to be retrieved. Wildcards (*) are supported");

    public GetGame()
        : base("game", "Retrieves information for current games")
    {
        AddAlias("games");
        AddArgument(GameName);
    }
}

internal sealed class GetGameHandler(ResourceGetter<RemoteGame> gameRetriever, ILogger<GetGameHandler> logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Game.SpanNameGet);
        var name = context.ParseResult.GetValueForArgument(GetGame.GameName);
        var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
        var cancellationToken = context.GetCancellationToken();

        var games = await gameRetriever.GetResources(name, cancellationToken).ConfigureAwait(false);

        if (!games.IsValid)
        {
            games.LogErrors(logger);
            return 1;
        }

        gameRetriever.PrintResources(games.Value, Console.Out, windowWidth);
        return 0;
    }
}
