using System.Globalization;

namespace Worms.Armageddon.Game.Replays;

internal sealed class ReplayFrameExtractor : IReplayFrameExtractor
{
    private readonly IWormsRunner _wormsRunner;
    private const string TimeFormatString = @"hh\:mm\:ss\.ff";

    public ReplayFrameExtractor(IWormsRunner wormsRunner) => _wormsRunner = wormsRunner;

    public async Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480)
    {
        var start = startTime.ToString(TimeFormatString, CultureInfo.CurrentCulture);
        var end = endTime.ToString(TimeFormatString, CultureInfo.CurrentCulture);

        await _wormsRunner.RunWorms("/getvideo",
            $"\"{replayPath}\"",
            fps.ToString(CultureInfo.CurrentCulture),
            start,
            end,
            xResolution.ToString(CultureInfo.CurrentCulture),
            yResolution.ToString(CultureInfo.CurrentCulture),
            "/quiet");
    }
}
