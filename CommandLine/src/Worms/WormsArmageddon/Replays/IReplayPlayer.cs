using System.Threading.Tasks;

namespace Worms.WormsArmageddon.Replays
{
    public interface IReplayPlayer
    {
        Task Play(string replayPath);
    }
}
