using System.IO.Abstractions;
using ImageMagick;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;

namespace Worms.Cli.Resources.Local.Gifs
{
    public class LocalGifCreator : IResourceCreator<LocalGif, LocalGifCreateParameters>
    {
        private readonly IReplayFrameExtractor _replayFrameExtractor;
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalGifCreator(
            IReplayFrameExtractor replayFrameExtractor,
            IWormsLocator wormsLocator,
            IFileSystem fileSystem)
        {
            _replayFrameExtractor = replayFrameExtractor;
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
        }

        public async Task<LocalGif> Create(LocalGifCreateParameters parameters, ILogger logger, CancellationToken cancellationToken)
        {
            var replayPath = parameters.Replay.Paths.WAgamePath;
            var turn = parameters.Replay.Details.Turns.ElementAt((int) parameters.Turn - 1);

            var replayName = _fileSystem.Path.GetFileNameWithoutExtension(replayPath);
            var worms = _wormsLocator.Find();
            var framesFolder = _fileSystem.Path.Combine(worms.CaptureFolder, replayName);
            var outputFileName = _fileSystem.Path.Combine(worms.CaptureFolder, replayName + " - " + parameters.Turn + ".gif");

            var animationDelay = 100 / parameters.FramesPerSecond / parameters.SpeedMultiplier;

            DeleteFrames(framesFolder);
            await _replayFrameExtractor.ExtractReplayFrames(
                replayPath,
                parameters.FramesPerSecond,
                turn.Start + parameters.StartOffset,
                turn.End - parameters.EndOffset);
            CreateGifFromFiles(framesFolder, outputFileName, animationDelay, 640, 480);
            DeleteFrames(framesFolder);

            return new LocalGif(outputFileName);
        }

        private void DeleteFrames(string framesFolder)
        {
            if (_fileSystem.Directory.Exists(framesFolder))
            {
                _fileSystem.Directory.Delete(framesFolder, true);
            }
        }

        private void CreateGifFromFiles(
            string framesFolder,
            string outputFile,
            uint animationDelay,
            int width,
            int height)
        {
            var frames = _fileSystem.Directory.GetFiles(framesFolder, "*.png");

            using var collection = new MagickImageCollection();
            foreach (var file in frames)
            {
                var image = new MagickImage(file);
                image.Resize(width, height);
                image.AnimationDelay = (int) animationDelay;
                collection.Add(image);
            }

            var settings = new QuantizeSettings { Colors = 256 };

            collection.Quantize(settings);
            collection.OptimizeTransparency();
            collection.Write(outputFile);
        }
    }
}
