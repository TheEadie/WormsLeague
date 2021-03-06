using System;
using System.Threading.Tasks;

namespace Worms.Armageddon.Game.Replays
{
    internal class ReplayPlayer : IReplayPlayer
    {
        private readonly IWormsRunner _wormsRunner;

        public ReplayPlayer(IWormsRunner wormsRunner)
        {
            _wormsRunner = wormsRunner;
        }

        public async Task Play(string replayPath)
        {
            await _wormsRunner.RunWorms("/play", $"\"{replayPath}\"", "/quiet");
        }

        public async Task Play(string replayPath, TimeSpan startTime)
        {
            await _wormsRunner.RunWorms("/playat", $"\"{replayPath}\"", startTime.ToString(), "/quiet");
        }
    }
}
