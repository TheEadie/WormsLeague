namespace Worms.Armageddon.Game.Replays;

public interface IReplayLogGenerator
{
    Task GenerateReplayLog(string replayPath);
}
