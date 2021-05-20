using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Game.Replays;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "Extract more information from replays (.WAgame files)")]
    internal class ProcessReplay : CommandBase
    {
        private readonly IReplayLogGenerator _replayLogGenerator;
        private readonly IResourceRetriever<LocalReplay> _replayRetriever;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Replay to be processed. Wildcards (*) are supported")]
        public string Name { get; }

        public ProcessReplay(IReplayLogGenerator replayLogGenerator, IResourceRetriever<LocalReplay> replayRetriever)
        {
            _replayLogGenerator = replayLogGenerator;
            _replayRetriever = replayRetriever;
        }

        public async Task<int> OnExecuteAsync()
        {
            var pattern = string.Empty;

            if (Name != "*" && !string.IsNullOrEmpty(Name))
            {
                pattern = Name;
            }

            foreach (var replayPath in _replayRetriever.Get(pattern))
            {
                Logger.Information($"Processing: {replayPath.Paths.WAgamePath}");
                await _replayLogGenerator.GenerateReplayLog(replayPath.Paths.WAgamePath);
            }

            return 0;
        }
    }
}
