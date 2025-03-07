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
    public static readonly Argument<string> ReplayName = new("name", "The name of the Replay to be viewed");

    public static readonly Option<uint> Turn = new(
        [
            "--turn",
            "-t"
        ],
        "The turn you wish to start the replay from");

    public ViewReplay()
        : base("replay", "View replays (.WAgame file)")
    {
        AddAlias("replays");
        AddAlias("WAgame");
        AddArgument(ReplayName);
        AddOption(Turn);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ViewReplayHandler(
    ResourceViewer<LocalReplay, LocalReplayViewParameters> resourceViewer,
    ILogger<ViewReplay> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameView);
        var name = context.ParseResult.GetValueForArgument(ViewReplay.ReplayName);
        var turn = context.ParseResult.GetValueForOption(ViewReplay.Turn);
        var cancellationToken = context.GetCancellationToken();

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
