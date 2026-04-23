using Worms.Armageddon.Game;
using Worms.Armageddon.Gifs;

namespace Worms.Cli.Resources.Local.Gifs;

internal sealed class LocalGifCreator(GifCreator gifCreator, IWormsArmageddon wormsArmageddon)
    : IResourceCreator<LocalGif, LocalGifCreateParameters>
{
    public async Task<LocalGif> Create(LocalGifCreateParameters parameters, CancellationToken cancellationToken)
    {
        var replayPath = parameters.Replay.Paths.WAgamePath;
        var turn = parameters.Replay.Details.Turns.ElementAt((int) parameters.Turn - 1);
        var outputFolder = wormsArmageddon.FindInstallation().CaptureFolder;

        var gifFileName = await gifCreator.CreateGif(
            replayPath,
            turn.Start,
            turn.End,
            (int) parameters.Turn,
            outputFolder,
            parameters.FramesPerSecond,
            parameters.SpeedMultiplier,
            parameters.StartOffset,
            parameters.EndOffset);

        return new LocalGif(Path.Combine(outputFolder, gifFileName));
    }
}
