using System;
using System.Threading.Tasks;

namespace Worms.Armageddon.Game.Replays
{
    public interface IReplayFrameExtractor
    {
        Task ExtractReplayFrames(
            string replayPath,
            int fps,
            TimeSpan startTime,
            TimeSpan endTime,
            int xResolution = 640,
            int yResolution = 480);
    }
}
