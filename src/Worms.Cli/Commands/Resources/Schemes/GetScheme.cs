using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Cli.Commands.Resources.Schemes;

internal sealed class GetScheme : Command
{
    public static readonly Argument<string> SchemeName = new(
        "name",
        () => "",
        "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported");

    public GetScheme()
        : base("scheme", "Retrieves information for Worms Schemes (.wsc files)")
    {
        AddAlias("schemes");
        AddAlias("wsc");
        AddArgument(SchemeName);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GetSchemeHandler(ResourceGetter<LocalScheme> schemesRetriever, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) => Task.Run(async () => await InvokeAsync(context)).Result;

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(GetScheme.SchemeName);
        var cancellationToken = context.GetCancellationToken();

        try
        {
            var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
            await schemesRetriever.PrintResources(name, Console.Out, windowWidth, logger, cancellationToken);
        }
        catch (ConfigurationException exception)
        {
            logger.Error(exception.Message);
            return 1;
        }

        return 0;
    }
}
