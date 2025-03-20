namespace Worms.Armageddon.Game.Fake;

internal sealed class NotInstalled : IWormsArmageddon
{
    public GameInfo FindInstallation() => GameInfo.NotInstalled;

    public Task Host() => Task.Run(() => throw new InvalidOperationException("Worms Armageddon is not installed"));

    public Task GenerateReplayLog(string replayPath) =>
        Task.Run(() => throw new InvalidOperationException("Worms Armageddon is not installed"));

    public Task PlayReplay(string replayPath) =>
        Task.Run(() => throw new InvalidOperationException("Worms Armageddon is not installed"));

    public Task PlayReplay(string replayPath, TimeSpan startTime) =>
        Task.Run(() => throw new InvalidOperationException("Worms Armageddon is not installed"));

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480) =>
        Task.Run(() => throw new InvalidOperationException("Worms Armageddon is not installed"));
}
