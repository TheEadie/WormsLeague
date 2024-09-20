using System.Globalization;

namespace Worms.Armageddon.Game.Replays;

internal sealed class ReplayFrameExtractor(IWormsRunner wormsRunner) : IReplayFrameExtractor
{
    private const string TimeFormatString = @"hh\:mm\:ss\.ff";

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480)
    {
        var start = startTime.ToString(TimeFormatString, CultureInfo.CurrentCulture);
        var end = endTime.ToString(TimeFormatString, CultureInfo.CurrentCulture);

        return wormsRunner.RunWorms(
            "/getvideo",
            $"\"{replayPath}\"",
            fps.ToString(CultureInfo.CurrentCulture),
            start,
            end,
            xResolution.ToString(CultureInfo.CurrentCulture),
            yResolution.ToString(CultureInfo.CurrentCulture),
            "/quiet");
    }
}
