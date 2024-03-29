using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Update : Command
{
    public Update()
        : base("update", "Update Worms CLI") { }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class UpdateHandler(CliUpdater cliUpdater, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        await cliUpdater.DownloadLatestUpdate(logger).ConfigureAwait(false);
        return 0;
    }
}
