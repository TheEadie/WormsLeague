using System.Globalization;

namespace Worms.Armageddon.Game;

internal sealed class WormsArmageddon(IWormsRunner wormsRunner) : IWormsArmageddon
{
    private const string TimeFormatString = @"hh\:mm\:ss\.ff";

    public Task Host() => wormsRunner.RunWorms("wa://");

    public Task GenerateReplayLog(string replayPath) => wormsRunner.RunWorms("/getlog", $"\"{replayPath}\"", "/quiet");

    public Task PlayReplay(string replayPath) => wormsRunner.RunWorms("/play", $"\"{replayPath}\"", "/quiet");

    public Task PlayReplay(string replayPath, TimeSpan startTime) =>
        wormsRunner.RunWorms("/playat", $"\"{replayPath}\"", startTime.ToString(), "/quiet");

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
