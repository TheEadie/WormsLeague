using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;

namespace Worms.Cli.Resources.Local.Gifs
{
    public class LocalGifCreator : IResourceCreator<LocalGifCreateParameters>
    {
        private readonly IReplayFrameExtractor _replayFrameExtractor;
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalGifCreator(IReplayFrameExtractor replayFrameExtractor, IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _replayFrameExtractor = replayFrameExtractor;
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
        }

        public async Task Create(LocalGifCreateParameters parameters)
        {
            var replayPath = parameters.Replay.Paths.WAgamePath;
            var turn = parameters.Replay.Details.Turns.ElementAt((int)parameters.Turn - 1);

            var replayName = _fileSystem.Path.GetFileNameWithoutExtension(replayPath);
            DeleteFrames(replayName);
            await _replayFrameExtractor.ExtractReplayFrames(replayPath, 30, turn.Start, turn.End);
        }

        private void DeleteFrames(string replayName)
        {
            var worms = _wormsLocator.Find();
            var framesFolder = _fileSystem.Path.Combine(worms.CaptureFolder, replayName);

            if (_fileSystem.Directory.Exists(framesFolder))
            {
                _fileSystem.Directory.Delete(framesFolder, true);
            }
        }
    }
}
