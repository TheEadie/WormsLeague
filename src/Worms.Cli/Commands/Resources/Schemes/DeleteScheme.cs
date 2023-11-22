using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
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
internal sealed class DeleteSchemeHandler(ResourceDeleter<LocalScheme> resourceDeleter, ILogger logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).Result;

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(DeleteScheme.SchemeName);
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
