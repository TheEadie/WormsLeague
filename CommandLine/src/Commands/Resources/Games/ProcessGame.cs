using System.IO.Abstractions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon;
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
        private readonly IFileSystem _fileSystem;
        private readonly IWormsLocator _wormsLocator;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Game to be processed. Wildcards (*) are supported")]
        public string Name { get; }

        public ProcessGame(IReplayLogGenerator replayLogGenerator, IFileSystem fileSystem, IWormsLocator wormsLocator)
        {
            _replayLogGenerator = replayLogGenerator;
            _fileSystem = fileSystem;
            _wormsLocator = wormsLocator;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            var gameInfo = _wormsLocator.Find();

            var pattern = string.Empty;

            if (Name != "*" && !string.IsNullOrEmpty(Name))
            {
                pattern = Name;
            }

            foreach (var game in _fileSystem.Directory.GetFiles(gameInfo.GamesFolder, $"{pattern}*.WAgame"))
            {
                Logger.Information($"Processing: {game}");
                await _replayLogGenerator.GenerateReplayLog(_fileSystem.Path.Combine(gameInfo.GamesFolder, game));
            }

            return 0;
        }
    }
}
