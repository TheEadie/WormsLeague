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
    public static readonly Argument<string> SchemeName = new("name", "The name of the Scheme to be deleted");

    public DeleteScheme()
        : base("scheme", "Delete Worms Schemes (.wsc files)")
    {
        AddAlias("schemes");
        AddAlias("wsc");
        AddArgument(SchemeName);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DeleteSchemeHandler(
    ResourceDeleter<LocalScheme> resourceDeleter,
    ILogger<DeleteSchemeHandler> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Scheme.SpanNameDelete);
        var name = context.ParseResult.GetValueForArgument(DeleteScheme.SchemeName);
        var cancellationToken = context.GetCancellationToken();

        var scheme = await resourceDeleter.GetResource(name, cancellationToken).ConfigureAwait(false);

        if (!scheme.IsValid)
        {
            scheme.LogErrors(logger);
            return 1;
        }

        resourceDeleter.Delete(scheme.Value);
        return 0;
    }
}
