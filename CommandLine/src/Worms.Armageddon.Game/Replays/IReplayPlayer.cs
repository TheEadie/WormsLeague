using System.Threading.Tasks;

namespace Worms.Armageddon.Game.Replays
{
    public interface IReplayPlayer
    {
        Task Play(string replayPath);
    }
}
