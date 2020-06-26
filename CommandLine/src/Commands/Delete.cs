using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("delete", "rm", Description = "Delete a resource")]
    [Subcommand(typeof(DeleteScheme))]
    internal class Delete : CommandBase
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
