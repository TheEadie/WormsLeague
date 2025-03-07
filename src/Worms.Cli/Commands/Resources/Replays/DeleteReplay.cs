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
    public static readonly Argument<string> ReplayName = new("name", "The name of the Replay to be deleted");

    public DeleteReplay()
        : base("replay", "Delete replays (.WAgame files)")
    {
        AddAlias("replays");
        AddAlias("WAgame");
        AddArgument(ReplayName);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DeleteReplayHandler(
    ResourceDeleter<LocalReplay> resourceDeleter,
    ILogger<DeleteReplayHandler> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameDelete);
        var name = context.ParseResult.GetValueForArgument(DeleteReplay.ReplayName);
        var cancellationToken = context.GetCancellationToken();

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
