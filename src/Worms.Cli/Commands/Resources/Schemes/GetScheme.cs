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
    public static readonly Argument<string> SchemeName = new("name")
    {
        Description =
            "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported",
        DefaultValueFactory = _ => ""
    };

    public GetScheme()
        : base("scheme", "Retrieves information for Worms Schemes (.wsc files)")
    {
        Aliases.Add("schemes");
        Aliases.Add("wsc");
        Arguments.Add(SchemeName);
    }
}

internal sealed class GetSchemeHandler(ResourceGetter<LocalScheme> schemesRetriever, ILogger<GetSchemeHandler> logger)
    : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Scheme.SpanNameGet);
        var name = parseResult.GetRequiredValue(GetScheme.SchemeName);
        var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;

        var schemes = await schemesRetriever.GetResources(name, cancellationToken);

        if (!schemes.IsValid)
        {
            schemes.LogErrors(logger);
            return 1;
        }

        schemesRetriever.PrintResources(schemes.Value, Console.Out, windowWidth);
        return 0;
    }
}
