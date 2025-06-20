using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class ProcessReplay : Command
{
    public static readonly Argument<string> ReplayName = new("name")
    {
        Description =
            "Optional: The name or search pattern for the Replay to be processed. Wildcards (*) are supported",
        DefaultValueFactory = _ => ""
    };

    public ProcessReplay()
        : base("replay", "Extract more information from replays (.WAgame files)")
    {
        Aliases.Add("replays");
        Aliases.Add("WAgame");
        Arguments.Add(ReplayName);
    }
}

internal sealed class ProcessReplayHandler(
    IWormsArmageddon wormsArmageddon,
    IResourceRetriever<LocalReplay> replayRetriever,
    ILogger<ProcessReplayHandler> logger) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameProcess);
        var name = parseResult.GetValue(ProcessReplay.ReplayName);

        var pattern = string.Empty;

        if (name != "*" && !string.IsNullOrEmpty(name))
        {
            pattern = name;
        }

        foreach (var replayPath in await replayRetriever.Retrieve(pattern, cancellationToken))
        {
            logger.LogInformation("Processing: {Path}", replayPath.Paths.WAgamePath);
            await wormsArmageddon.GenerateReplayLog(replayPath.Paths.WAgamePath);
        }

        return 0;
    }
}
