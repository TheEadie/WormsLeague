using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Version() : Command("version", "Get the current version of the Worms CLI");

internal sealed class VersionHandler(IWormsLocator wormsLocator, CliInfoRetriever cliInfoRetriever) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Version.SpanName);

        var (cliVersion, gameVersion) = GetVersions();

        context.Console.WriteLine($"Worms CLI: {cliVersion.ToString(3)}");
        context.Console.WriteLine($"Worms Armageddon: {gameVersion?.ToString(4) ?? "Not Installed"}");

        _ = Activity.Current?.SetTag(Telemetry.Spans.Version.CliVersion, cliVersion);
        _ = Activity.Current?.SetTag(Telemetry.Spans.Version.WormsArmageddonVersion, gameVersion);
        return Task.FromResult(0);
    }

    private (System.Version CliVersion, System.Version? GameVersion) GetVersions()
    {
        var cliInfo = cliInfoRetriever.Get();
        var gameInfo = wormsLocator.Find();
        return (cliInfo.Version, gameInfo.IsInstalled ? gameInfo.Version : null);
    }
}
