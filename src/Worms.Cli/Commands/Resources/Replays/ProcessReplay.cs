using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game.Replays;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class ProcessReplay : Command
{
    public static readonly Argument<string> ReplayName = new(
        "name",
        () => "",
        "Optional: The name or search pattern for the Replay to be processed. Wildcards (*) are supported");

    public ProcessReplay()
        : base("replay", "Extract more information from replays (.WAgame files)")
    {
        AddAlias("replays");
        AddAlias("WAgame");
        AddArgument(ReplayName);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ProcessReplayHandler(
    IReplayLogGenerator replayLogGenerator,
    IResourceRetriever<LocalReplay> replayRetriever,
    ILogger<ProcessReplayHandler> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(ProcessReplay.ReplayName);
        var cancellationToken = context.GetCancellationToken();

        var pattern = string.Empty;

        if (name != "*" && !string.IsNullOrEmpty(name))
        {
            pattern = name;
        }

        foreach (var replayPath in await replayRetriever.Retrieve(pattern, cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Processing: {Path}", replayPath.Paths.WAgamePath);
            await replayLogGenerator.GenerateReplayLog(replayPath.Paths.WAgamePath).ConfigureAwait(false);
        }

        return 0;
    }
}
