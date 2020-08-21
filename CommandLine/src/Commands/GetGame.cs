using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Resources;
using Worms.Resources.Games;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("game", "games", "replay", "replays", "WAgame", Description = "Retrieves information for Worms games (.WAgame files)")]
    internal class GetGame : CommandBase
    {
        private readonly ResourceGetter<GameResource> _gameRetriever;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Game to be retrieved. Wildcards (*) are supported")]
        public string Name { get; }

        public GetGame(ResourceGetter<GameResource> gameRetriever)
        {
            _gameRetriever = gameRetriever;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                _gameRetriever.PrintResources(Name, console.Out, windowWidth);
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
