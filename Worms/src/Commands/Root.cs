using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Worms.Commands
{
    [Command("worms", Description = "Worms CLI"),
        Subcommand(typeof(Version)),
        Subcommand(typeof(Host)),
    ]
    public class Root
    {
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp();
            return Task.FromResult(1);
        }
    }
}