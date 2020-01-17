using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Worms.Commands
{
    [Command("worms", Description = "Worms CLI"),
        Subcommand(typeof(Version)),
        Subcommand(typeof(Update)),
        Subcommand(typeof(Host)),
        Subcommand(typeof(Setup)),
    ]
    public class Root : CommandBase
    {
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp();
            return Task.FromResult(1);
        }
    }
}