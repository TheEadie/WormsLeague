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

        public Task<int> OnExecuteAsync(IConsole console)
        {
            var cliVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
            console.WriteLine($"Worms CLI: {cliVersion}");

            var gameInfo = _wormsLocator.Find();
            var gameVersion = gameInfo.IsInstalled ? gameInfo.Version.ToString(4) : "Not Installed";
            console.WriteLine($"Worms Armageddon: {gameVersion}");
            return Task.FromResult(0);
        }
    }
}