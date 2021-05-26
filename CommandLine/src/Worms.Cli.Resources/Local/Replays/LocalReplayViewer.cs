using System.Linq;
using System.Threading.Tasks;
using Worms.Armageddon.Game.Replays;

namespace Worms.Cli.Resources.Local.Replays
{
    internal class LocalReplayViewer : IResourceViewer<LocalReplay, LocalReplayViewParameters>
    {
        private readonly IReplayPlayer _replayPlayer;

        public LocalReplayViewer(IReplayPlayer replayPlayer)
        {
            _replayPlayer = replayPlayer;
        }

        public async Task View(LocalReplay resource, LocalReplayViewParameters parameters)
        {
            if (parameters.Turn != default)
            {
                var startTime = resource.Details.Turns.ElementAt((int)parameters.Turn - 1).Start;
                await _replayPlayer.Play(resource.Paths.WAgamePath, startTime);
                return;
            }

            await _replayPlayer.Play(resource.Paths.WAgamePath);
        }
    }
}
