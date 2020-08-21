using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Commands.Resources.Schemes;

// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources
{
    [Command("create", Description = "Create a resource")]
    [Subcommand(typeof(CreateScheme))]
    internal class Create : CommandBase
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
