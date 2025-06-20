using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Version() : Command("version", "Get the current version of the Worms CLI");

internal sealed class VersionHandler(IWormsArmageddon wormsArmageddon, CliInfoRetriever cliInfoRetriever)
    : AsynchronousCommandLineAction
{
    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Version.SpanName);

        var (cliVersion, gameVersion) = GetVersions();

        Console.WriteLine($"Worms CLI: {cliVersion.ToString(3)}");
        Console.WriteLine($"Worms Armageddon: {gameVersion?.ToString(4) ?? "Not Installed"}");

        _ = Activity.Current?.SetTag(Telemetry.Spans.Version.CliVersion, cliVersion);
        _ = Activity.Current?.SetTag(Telemetry.Spans.Version.WormsArmageddonVersion, gameVersion);
        return Task.FromResult(0);
    }

    private (System.Version CliVersion, System.Version? GameVersion) GetVersions()
    {
        var cliInfo = cliInfoRetriever.Get();
        var gameInfo = wormsArmageddon.FindInstallation();
        return (cliInfo.Version, gameInfo.IsInstalled ? gameInfo.Version : null);
    }
}
