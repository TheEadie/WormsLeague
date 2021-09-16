using System.Linq;
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

        public void Create(LocalGifCreateParameters parameters)
        {
            var replayPath = parameters.Replay.Paths.WAgamePath;
            var turn = parameters.Replay.Details.Turns.ElementAt(parameters.Turn - 1);
            _replayFrameExtractor.ExtractReplayFrames(replayPath, 3, turn.Start, turn.End);
        }
    }
}
