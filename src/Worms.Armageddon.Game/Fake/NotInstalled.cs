namespace Worms.Armageddon.Game.Fake;

internal sealed class NotInstalled : IWormsArmageddon
{
    public GameInfo FindInstallation() => GameInfo.NotInstalled;

    public Task Host() => Task.Run(() => throw new InvalidOperationException("Worms Armageddon is not installed"));

    public Task GenerateReplayLog(string replayPath) => throw new NotImplementedException();

    public Task PlayReplay(string replayPath) => throw new NotImplementedException();

    public Task PlayReplay(string replayPath, TimeSpan startTime) => throw new NotImplementedException();

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480) =>
        throw new NotImplementedException();
}
