namespace Worms.Armageddon.Game.Replays;

internal sealed class ReplayPlayer(IWormsRunner wormsRunner) : IReplayPlayer
{
    public Task Play(string replayPath) => wormsRunner.RunWorms("/play", $"\"{replayPath}\"", "/quiet");

    public Task Play(string replayPath, TimeSpan startTime) =>
        wormsRunner.RunWorms("/playat", $"\"{replayPath}\"", startTime.ToString(), "/quiet");
}
