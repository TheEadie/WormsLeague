using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Resources;
using Worms.Resources.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "Retrieves information for Worms replays (.WAgame files)")]
    internal class GetReplay : CommandBase
    {
        private readonly ResourceGetter<ReplayResource> _replayRetriever;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Replay to be retrieved. Wildcards (*) are supported")]
        public string Name { get; }

        public GetReplay(ResourceGetter<ReplayResource> replayRetriever)
        {
            _replayRetriever = replayRetriever;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                _replayRetriever.PrintResources(Name, console.Out, windowWidth);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }
    }
}
