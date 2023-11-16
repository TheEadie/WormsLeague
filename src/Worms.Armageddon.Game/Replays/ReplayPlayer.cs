namespace Worms.Armageddon.Game.Replays;

internal sealed class ReplayPlayer(IWormsRunner wormsRunner) : IReplayPlayer
{
    public async Task Play(string replayPath) => await wormsRunner.RunWorms("/play", $"\"{replayPath}\"", "/quiet");

    public async Task Play(string replayPath, TimeSpan startTime) =>
        await wormsRunner.RunWorms("/playat", $"\"{replayPath}\"", startTime.ToString(), "/quiet");
}
