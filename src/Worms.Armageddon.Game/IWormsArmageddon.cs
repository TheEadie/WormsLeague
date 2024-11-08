namespace Worms.Armageddon.Game;

public interface IWormsArmageddon
{
    Task Host();

    Task GenerateReplayLog(string replayPath);

    Task PlayReplay(string replayPath);

    Task PlayReplay(string replayPath, TimeSpan startTime);

    Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480);
}
