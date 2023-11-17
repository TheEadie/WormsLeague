namespace Worms.Armageddon.Game.Replays;

public class ReplayLogGenerator(IWormsRunner wormsRunner) : IReplayLogGenerator
{
    public async Task GenerateReplayLog(string replayPath) =>
        await wormsRunner.RunWorms("/getlog", $"\"{replayPath}\"", "/quiet");
}
