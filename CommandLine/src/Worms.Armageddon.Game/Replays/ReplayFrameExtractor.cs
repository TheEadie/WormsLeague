using System;
using System.Threading.Tasks;

namespace Worms.Armageddon.Game.Replays
{
    internal class ReplayFrameExtractor : IReplayFrameExtractor
    {
        private readonly IWormsRunner _wormsRunner;
        private const string TimeFormatString = @"hh\:mm\:ss\.ff";

        public ReplayFrameExtractor(IWormsRunner wormsRunner)
        {
            _wormsRunner = wormsRunner;
        }

        public async Task ExtractReplayFrames(
            string replayPath,
            uint fps,
            TimeSpan startTime,
            TimeSpan endTime,
            int xResolution = 640,
            int yResolution = 480)
        {
            var start = startTime.ToString(TimeFormatString);
            var end = endTime.ToString(TimeFormatString);

            await _wormsRunner.RunWorms("/getvideo",
                $"\"{replayPath}\"",
                fps.ToString(),
                start,
                end,
                xResolution.ToString(),
                yResolution.ToString(),
                "/quiet");
        }
    }
}
