using System.Threading.Tasks;

namespace Worms.WormsArmageddon.Replays
{
    public interface IReplayLogGenerator
    {
        Task GenerateReplayLog(string replayPath);
    }
}
