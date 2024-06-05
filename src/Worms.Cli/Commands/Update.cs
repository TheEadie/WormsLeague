using System.CommandLine;
using System.CommandLine.Invocation;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Update : Command
{
    public static readonly Option<bool> Force = new(
        [
            "--force",
            "-f"
        ],
        "Forces the latest version to be downloaded and installed even if the CLI is already up to date");

    public Update()
        : base("update", "Update Worms CLI") =>
        AddOption(Force);
}

internal sealed class UpdateHandler(CliUpdater cliUpdater) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.Update.SpanName);

        var force = context.ParseResult.GetValueForOption(Update.Force);
        _ = span?.AddTag(Telemetry.Spans.Update.Force, force);

        await cliUpdater.DownloadAndInstall(force).ConfigureAwait(false);

        return 0;
    }
}
