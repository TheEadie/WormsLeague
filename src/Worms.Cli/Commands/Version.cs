using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Version : Command
{
    public Version()
        : base("version", "Get the current version of the Worms CLI") { }
}

internal sealed class VersionHandler(IWormsLocator wormsLocator, CliInfoRetriever cliInfoRetriever, ILogger logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).Result;

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var cliInfo = cliInfoRetriever.Get(logger);
        logger.Information($"Worms CLI: {cliInfo.Version.ToString(3)}");

        var gameInfo = wormsLocator.Find();
        var gameVersion = gameInfo.IsInstalled ? gameInfo.Version.ToString(4) : "Not Installed";
        logger.Information($"Worms Armageddon: {gameVersion}");
        return Task.FromResult(0);
    }
}
