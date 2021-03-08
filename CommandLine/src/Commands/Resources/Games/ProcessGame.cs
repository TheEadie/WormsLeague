using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Games
{
    [Command("game", "games", "replay", "replays", "WAgame", Description = "Extract more information from Worms games (.WAgame files)")]
    internal class ProcessGame : CommandBase
    {
        private readonly IReplayLogGenerator _replayLogGenerator;
        private readonly IReplayLocator _replayLocator;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Game to be processed. Wildcards (*) are supported")]
        public string Name { get; }

        public ProcessGame(IReplayLogGenerator replayLogGenerator, IReplayLocator replayLocator)
        {
            _replayLogGenerator = replayLogGenerator;
            _replayLocator = replayLocator;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            var pattern = string.Empty;

            if (Name != "*" && !string.IsNullOrEmpty(Name))
            {
                pattern = Name;
            }

            foreach (var game in _replayLocator.GetReplayPaths(pattern))
            {
                Logger.Information($"Processing: {game}");
                await _replayLogGenerator.GenerateReplayLog(game);
            }

            return 0;
        }
    }
}
