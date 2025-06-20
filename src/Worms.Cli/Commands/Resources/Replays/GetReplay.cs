using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class GetReplay : Command
{
    public static readonly Argument<string> ReplayName = new("name")
    {
        Description =
            "Optional: The name or search pattern for the Replay to be retrieved. Wildcards (*) are supported",
        DefaultValueFactory = _ => ""
    };

    public GetReplay()
        : base("replay", "Retrieves information for Worms replays (.WAgame files)")
    {
        Aliases.Add("replays");
        Aliases.Add("WAgame");
        Arguments.Add(ReplayName);
    }
}

internal sealed class GetReplayHandler(ResourceGetter<LocalReplay> replayRetriever, ILogger<GetReplayHandler> logger)
    : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameGet);
        var name = parseResult.GetRequiredValue(GetReplay.ReplayName);
        var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;

        var replays = await replayRetriever.GetResources(name, cancellationToken);

        if (!replays.IsValid)
        {
            replays.LogErrors(logger);
            return 1;
        }

        replayRetriever.PrintResources(replays.Value, Console.Out, windowWidth);
        return 0;
    }
}
