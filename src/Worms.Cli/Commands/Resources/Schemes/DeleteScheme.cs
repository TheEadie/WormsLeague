using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Cli.Commands.Resources.Schemes;

internal sealed class DeleteScheme : Command
{
    public static readonly Argument<string> SchemeName = new("name")
    {
        Description = "The name of the Scheme to be deleted"
    };

    public DeleteScheme()
        : base("scheme", "Delete Worms Schemes (.wsc files)")
    {
        Aliases.Add("schemes");
        Aliases.Add("wsc");
        Arguments.Add(SchemeName);
    }
}

internal sealed class DeleteSchemeHandler(
    ResourceDeleter<LocalScheme> resourceDeleter,
    ILogger<DeleteSchemeHandler> logger) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Scheme.SpanNameDelete);
        var name = parseResult.GetRequiredValue(DeleteScheme.SchemeName);

        var scheme = await resourceDeleter.GetResource(name, cancellationToken);

        if (!scheme.IsValid)
        {
            scheme.LogErrors(logger);
            return 1;
        }

        resourceDeleter.Delete(scheme.Value);
        return 0;
    }
}
