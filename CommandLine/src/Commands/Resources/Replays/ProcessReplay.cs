using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "Extract more information from Worms games (.WAgame files)")]
    internal class ProcessReplay : CommandBase
    {
        private readonly IReplayLogGenerator _replayLogGenerator;
        private readonly IReplayLocator _replayLocator;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Replay to be processed. Wildcards (*) are supported")]
        public string Name { get; }

        public ProcessReplay(IReplayLogGenerator replayLogGenerator, IReplayLocator replayLocator)
        {
            _replayLogGenerator = replayLogGenerator;
            _replayLocator = replayLocator;
        }

        public async Task<int> OnExecuteAsync()
        {
            var pattern = string.Empty;

            if (Name != "*" && !string.IsNullOrEmpty(Name))
            {
                pattern = Name;
            }

            foreach (var replayPath in _replayLocator.GetReplayPaths(pattern))
            {
                Logger.Information($"Processing: {replayPath}");
                await _replayLogGenerator.GenerateReplayLog(replayPath);
            }

            return 0;
        }
    }
}
