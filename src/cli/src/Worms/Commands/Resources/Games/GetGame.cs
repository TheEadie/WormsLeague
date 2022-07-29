using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Cli.Resources.Remote.Games;
using Worms.Resources;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Games
{
    [Command("game", "games", Description = "Retrieves information for current games")]
    internal class GetGame : CommandBase
    {
        private readonly ResourceGetter<RemoteGame> _gameRetriever;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Game to be retrieved. Wildcards (*) are supported")]
        public string Name { get; }

        public GetGame(ResourceGetter<RemoteGame> gameRetriever)
        {
            _gameRetriever = gameRetriever;
        }

        public async Task<int> OnExecuteAsync(IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                await _gameRetriever.PrintResources(Name, console.Out, windowWidth, Logger, cancellationToken);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            return 0;
        }
    }
}
