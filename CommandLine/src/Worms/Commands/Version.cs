using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Game;
using Worms.Cli;

// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("version", Description = "Get the current version of the worms CLI")]
    internal class Version : CommandBase
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly CliInfoRetriever _cliInfoRetriever;

        public Version(IWormsLocator wormsLocator, CliInfoRetriever cliInfoRetriever)
        {
            _wormsLocator = wormsLocator;
            _cliInfoRetriever = cliInfoRetriever;
        }

        public Task<int> OnExecuteAsync()
        {
            var cliInfo = _cliInfoRetriever.Get();
            Logger.Information($"Worms CLI: {cliInfo.Version.ToString(3)}");

            var gameInfo = _wormsLocator.Find();
            var gameVersion = gameInfo.IsInstalled ? gameInfo.Version.ToString(4) : "Not Installed";
            Logger.Information($"Worms Armageddon: {gameVersion}");
            return Task.FromResult(0);
        }
    }
}
