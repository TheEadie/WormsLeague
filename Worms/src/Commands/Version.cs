using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Worms.Commands
{
    [Command("version", Description = "Get the current version of the worms CLI")]
    public class Version
    {
        public Task<int> OnExecuteAsync(IConsole console)
        {
            var versionString = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
            console.WriteLine($"Worms {versionString}");
            return Task.FromResult(0);
        }
    }
}