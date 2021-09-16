using System.Linq;
using System.Threading.Tasks;
using Worms.Armageddon.Game.Replays;

namespace Worms.Cli.Resources.Local.Gifs
{
    public class LocalGifCreator : IResourceCreator<LocalGifCreateParameters>
    {
        private readonly IReplayFrameExtractor _replayFrameExtractor;

        public LocalGifCreator(IReplayFrameExtractor replayFrameExtractor)
        {
            _replayFrameExtractor = replayFrameExtractor;
        }

        public async Task Create(LocalGifCreateParameters parameters)
        {
            var replayPath = parameters.Replay.Paths.WAgamePath;
            var turn = parameters.Replay.Details.Turns.ElementAt((int)parameters.Turn - 1);
            await _replayFrameExtractor.ExtractReplayFrames(replayPath, 3, turn.Start, turn.End);
        }
    }
}
