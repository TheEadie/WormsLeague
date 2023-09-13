namespace Worms.Armageddon.Game.Replays
{
    public interface IReplayPlayer
    {
        Task Play(string replayPath);

        Task Play(string replayPath, TimeSpan startTime);
    }
}
