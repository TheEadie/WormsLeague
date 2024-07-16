using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
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

internal sealed class GetSchemeHandler(ResourceGetter<LocalScheme> schemesRetriever, ILogger<GetSchemeHandler> logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Scheme.SpanNameGet);
        var name = context.ParseResult.GetValueForArgument(GetScheme.SchemeName);
        var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
        var cancellationToken = context.GetCancellationToken();

        var schemes = await schemesRetriever.GetResources(name, cancellationToken).ConfigureAwait(false);

        if (!schemes.IsValid)
        {
            schemes.LogErrors(logger);
            return 1;
        }

        schemesRetriever.PrintResources(schemes.Value, Console.Out, windowWidth);
        return 0;
    }
}
