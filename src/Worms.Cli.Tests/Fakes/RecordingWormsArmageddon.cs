using Worms.Armageddon.Game;

namespace Worms.Cli.Tests.Fakes;

internal sealed class RecordingWormsArmageddon(IWormsArmageddon inner) : IWormsArmageddon
{
    internal sealed record PlayReplayCall(string Path, TimeSpan? StartTime);

    public List<PlayReplayCall> PlayReplayCalls { get; } = [];

    public bool HostWasCalled { get; private set; }

    public GameInfo FindInstallation() => inner.FindInstallation();

    public Task Host()
    {
        HostWasCalled = true;
        return inner.Host();
    }

    public Task GenerateReplayLog(string replayPath) => inner.GenerateReplayLog(replayPath);

    public Task PlayReplay(string replayPath)
    {
        PlayReplayCalls.Add(new PlayReplayCall(replayPath, null));
        return inner.PlayReplay(replayPath);
    }

    public Task PlayReplay(string replayPath, TimeSpan startTime)
    {
        PlayReplayCalls.Add(new PlayReplayCall(replayPath, startTime));
        return inner.PlayReplay(replayPath, startTime);
    }

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480) =>
        inner.ExtractReplayFrames(replayPath, fps, startTime, endTime, xResolution, yResolution);
}
