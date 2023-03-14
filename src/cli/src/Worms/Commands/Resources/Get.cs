using System.CommandLine;
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
    internal class Get : Command
    {
        public Get() : base("get", "Get a list of resources")
        {
        }
    }
}