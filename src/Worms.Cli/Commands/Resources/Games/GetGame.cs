using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Remote.Games;

namespace Worms.Cli.Commands.Resources.Games;

internal sealed class GetGame : Command
{
    public static readonly Argument<string> GameName = new(
        "name",
        () => "",
        "Optional: The name or search pattern for the Game to be retrieved. Wildcards (*) are supported");

    public GetGame()
        : base("game", "Retrieves information for current games")
    {
        AddAlias("games");
        AddArgument(GameName);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GetGameHandler(ResourceGetter<RemoteGame> gameRetriever, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(GetGame.GameName);
        var cancellationToken = context.GetCancellationToken();

        try
        {
            var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
            await gameRetriever.PrintResources(name, Console.Out, windowWidth, logger, cancellationToken)
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
