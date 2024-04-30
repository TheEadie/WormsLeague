using System.CommandLine;
using System.CommandLine.Invocation;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Update : Command
{
    public static readonly Option<bool> Force = new(
        new[]
        {
            "--force",
            "-f"
        },
        "Forces the latest version to be downloaded and installed even if the CLI is already up to date");

    public Update()
        : base("update", "Update Worms CLI") =>
        AddOption(Force);
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class UpdateHandler(CliUpdater cliUpdater) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var force = context.ParseResult.GetValueForOption(Update.Force);

        await cliUpdater.DownloadAndInstall(force).ConfigureAwait(false);

        return 0;
    }
}
