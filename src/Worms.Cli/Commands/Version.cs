using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Version : Command
{
    public Version()
        : base("version", "Get the current version of the Worms CLI") { }
}

internal sealed class VersionHandler(
    IWormsLocator wormsLocator,
    CliInfoRetriever cliInfoRetriever,
    ILogger<VersionHandler> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var cliInfo = cliInfoRetriever.Get();
        logger.LogInformation("Worms CLI: {Version}", cliInfo.Version.ToString(3));

        var gameInfo = wormsLocator.Find();
        var gameVersion = gameInfo.IsInstalled ? gameInfo.Version.ToString(4) : "Not Installed";
        logger.LogInformation("Worms Armageddon: {Version}", gameVersion);
        return Task.FromResult(0);
    }
}
