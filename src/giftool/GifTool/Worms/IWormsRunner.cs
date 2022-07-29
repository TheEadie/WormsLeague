using System;
using System.Threading.Tasks;

namespace GifTool.Worms
{
    public interface IWormsRunner
    {
        Task<string> CreateReplayLog(string replay);
        Task<string[]> CreateReplayVideo(string replay, int frameRateDivider, TimeSpan start, TimeSpan end, int xResolution, int yResolution);
        string[] GetAllReplays();
        Turn[] ReadReplayLog(string replayLog);
        bool TryGetLogForReplay(string replay, out string replayLog);
    }
}