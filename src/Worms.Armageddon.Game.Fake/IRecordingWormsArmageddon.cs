namespace Worms.Armageddon.Game.Fake;

/// <summary>
/// The recording surface of the installed fake. Lets tests assert on the WA interactions the fake observed
/// without depending on the concrete fake type.
/// </summary>
public interface IRecordingWormsArmageddon : IWormsArmageddon
{
    IReadOnlyList<PlayReplayCall> PlayReplayCalls { get; }

    int HostCallCount { get; }
}
