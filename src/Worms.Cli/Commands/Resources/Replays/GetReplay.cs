using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
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

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GetReplayHandler(ResourceGetter<LocalReplay> replayRetriever, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(GetReplay.ReplayName);
        var cancellationToken = context.GetCancellationToken();

        try
        {
            var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
            await replayRetriever.PrintResources(name, Console.Out, windowWidth, logger, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ConfigurationException exception)
        {
            logger.Error(exception.Message);
            return 1;
        }

        return 0;
    }
}
