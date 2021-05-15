using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Commands.Resources.Replays;

// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources
{
    [Command("view", Description = "View a resource")]
    [Subcommand(typeof(ViewReplay))]
    internal class View : CommandBase
    {
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            Logger.Error("No resource type specified");
            Logger.Information("");
            app.ShowHelp();
            return Task.FromResult(1);
        }
    }
}
