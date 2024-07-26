namespace Worms.Armageddon.Game.Replays;

public class ReplayLogGenerator(IWormsRunner wormsRunner) : IReplayLogGenerator
{
    public Task GenerateReplayLog(string replayPath) => wormsRunner.RunWorms("/getlog", $"'{replayPath}'", "/quiet");
}
