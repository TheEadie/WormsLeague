using System.CommandLine;
using System.CommandLine.Invocation;
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
        using var span = Telemetry.Source.StartActivity("version");

        var (cliVersion, gameVersion) = GetVersions();

        context.Console.WriteLine($"Worms CLI: {cliVersion.ToString(3)}");
        context.Console.WriteLine($"Worms Armageddon: {gameVersion?.ToString(4) ?? "Not Installed"}");

        _ = span?.SetTag(Telemetry.Attributes.Version_CliVersion, cliVersion);
        _ = span?.SetTag(Telemetry.Attributes.Version_WormsArmageddonVersion, gameVersion);
        return Task.FromResult(0);
    }

    private (System.Version CliVersion, System.Version? GameVersion) GetVersions()
    {
        var cliInfo = cliInfoRetriever.Get();
        var gameInfo = wormsLocator.Find();
        return (cliInfo.Version, gameInfo.IsInstalled ? gameInfo.Version : null);
    }
}
