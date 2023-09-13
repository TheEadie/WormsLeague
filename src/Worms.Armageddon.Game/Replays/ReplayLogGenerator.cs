namespace Worms.Armageddon.Game.Replays
{
    public class ReplayLogGenerator : IReplayLogGenerator
    {
        private readonly IWormsRunner _wormsRunner;

        public ReplayLogGenerator(IWormsRunner wormsRunner)
        {
            _wormsRunner = wormsRunner;
        }

        public async Task GenerateReplayLog(string replayPath)
        {
            await _wormsRunner.RunWorms("/getlog", $"\"{replayPath}\"", "/quiet");
        }
    }
}
