using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class DeleteReplay : Command
{
    public static readonly Argument<string> ReplayName = new("name")
    {
        Description = "The name of the Replay to be deleted"
    };

    public DeleteReplay()
        : base("replay", "Delete replays (.WAgame files)")
    {
        Aliases.Add("replays");
        Aliases.Add("WAgame");
        Arguments.Add(ReplayName);
    }
}

internal sealed class DeleteReplayHandler(
    ResourceDeleter<LocalReplay> resourceDeleter,
    ILogger<DeleteReplayHandler> logger) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameDelete);
        var name = parseResult.GetRequiredValue(DeleteReplay.ReplayName);

        var replay = await resourceDeleter.GetResource(name, cancellationToken);

        if (!replay.IsValid)
        {
            replay.LogErrors(logger);
            return 1;
        }

        resourceDeleter.Delete(replay.Value);
        return 0;
    }
}
