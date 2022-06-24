using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Commands.Resources;

// ReSharper disable ClassNeverInstantiated.Global - CLI library loads this as the root command
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("worms", Description = "Worms CLI")]
    [Subcommand(typeof(Auth))]
    [Subcommand(typeof(Get))]
    [Subcommand(typeof(Create))]
    [Subcommand(typeof(Delete))]
    [Subcommand(typeof(Process))]
    [Subcommand(typeof(View))]
    [Subcommand(typeof(Version))]
    [Subcommand(typeof(Update))]
    [Subcommand(typeof(Host))]
    [Subcommand(typeof(Setup))]
    internal class Root : CommandBase
    {
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp();
            return Task.FromResult(1);
        }
    }
}
