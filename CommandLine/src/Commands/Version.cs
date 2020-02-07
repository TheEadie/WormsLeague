using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon;

namespace Worms.Commands
{
    [Command("version", Description = "Get the current version of the worms CLI")]
    internal class Version : CommandBase
    {
        private readonly IWormsLocator _wormsLocator;

        public Version(IWormsLocator wormsLocator)
        {
            _wormsLocator = wormsLocator;
        }

        public Task<int> OnExecuteAsync()
        {
            var cliVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
            Logger.Information($"Worms CLI: {cliVersion}");

            var gameInfo = _wormsLocator.Find();
            var gameVersion = gameInfo.IsInstalled ? gameInfo.Version.ToString(4) : "Not Installed";
            Logger.Information($"Worms Armageddon: {gameVersion}");
            return Task.FromResult(0);
        }
    }
}