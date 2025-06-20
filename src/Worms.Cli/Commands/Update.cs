using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Update : Command
{
    public static readonly Option<bool> Force = new("--force", "-f")
    {
        Description =
            "Forces the latest version to be downloaded and installed even if the CLI is already up to date"
    };

    public Update()
        : base("update", "Update Worms CLI") =>
        Options.Add(Force);
}

internal sealed class UpdateHandler(CliUpdater cliUpdater) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Update.SpanName);

        var force = parseResult.GetValue(Update.Force);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Update.Force, force);

        await cliUpdater.DownloadAndInstall(force);

        return 0;
    }
}
