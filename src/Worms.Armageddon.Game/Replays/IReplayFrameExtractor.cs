namespace Worms.Armageddon.Game.Replays
{
    public interface IReplayFrameExtractor
    {
        Task ExtractReplayFrames(
            string replayPath,
            uint fps,
            TimeSpan startTime,
            TimeSpan endTime,
            int xResolution = 640,
            int yResolution = 480);
    }
}
