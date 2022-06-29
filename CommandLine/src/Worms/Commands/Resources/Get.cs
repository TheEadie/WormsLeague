using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Commands.Resources.Games;
using Worms.Commands.Resources.Replays;
using Worms.Commands.Resources.Schemes;

// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources
{
    [Command("get", Description = "Get a list of resources")]
    [Subcommand(typeof(GetScheme))]
    [Subcommand(typeof(GetReplay))]
    [Subcommand(typeof(GetGame))]
    internal class Get : CommandBase
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
