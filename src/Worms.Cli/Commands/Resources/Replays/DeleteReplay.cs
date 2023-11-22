using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
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
internal sealed class DeleteReplayHandler(ResourceDeleter<LocalReplay> resourceDeleter, ILogger logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).Result;

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(DeleteReplay.ReplayName);
        var cancellationToken = context.GetCancellationToken();

        try
        {
            await resourceDeleter.Delete(name, logger, cancellationToken).ConfigureAwait(false);
        }
        catch (ConfigurationException exception)
        {
            logger.Error(exception.Message);
            return 1;
        }

        return 0;
    }
}
