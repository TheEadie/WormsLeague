using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class ViewReplay : Command
{
    public static readonly Argument<string> ReplayName =
        new("name") { Description = "The name of the Replay to be viewed" };

    public static readonly Option<uint> Turn = new("--turn", "-t")
    {
        Description = "The turn you wish to start the replay from"
    };

    public ViewReplay() : base("replay", "View replays (.WAgame file)")
    {
        Aliases.Add("replays");
        Aliases.Add("WAgame");
        Arguments.Add(ReplayName);
        Options.Add(Turn);
    }
}

internal sealed class ViewReplayHandler(
    ResourceViewer<LocalReplay, LocalReplayViewParameters> resourceViewer,
    ILogger<ViewReplay> logger) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameView);
        var name = parseResult.GetRequiredValue(ViewReplay.ReplayName);
        var turn = parseResult.GetValue(ViewReplay.Turn);

        var replay = await resourceViewer.GetResource(name, cancellationToken);

        if (!replay.IsValid)
        {
            replay.LogErrors(logger);
            return 1;
        }

        resourceViewer.View(replay.Value, new LocalReplayViewParameters(turn));
        return 0;
    }
}
