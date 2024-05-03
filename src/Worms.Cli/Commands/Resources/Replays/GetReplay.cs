using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class GetReplay : Command
{
    public static readonly Argument<string> ReplayName = new(
        "name",
        () => "",
        "Optional: The name or search pattern for the Replay to be retrieved. Wildcards (*) are supported");

    public GetReplay()
        : base("replay", "Retrieves information for Worms replays (.WAgame files)")
    {
        AddAlias("replays");
        AddAlias("WAgame");
        AddArgument(ReplayName);
    }
}

internal sealed class GetReplayHandler(ResourceGetter<LocalReplay> replayRetriever, ILogger<GetReplayHandler> logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(GetReplay.ReplayName);
        var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
        var cancellationToken = context.GetCancellationToken();

        var replays = await replayRetriever.GetResources(name, cancellationToken).ConfigureAwait(false);

        if (!replays.IsValid)
        {
            replays.LogErrors(logger);
            return 1;
        }

        replayRetriever.PrintResources(replays.Value, Console.Out, windowWidth);
        return 0;
    }
}
