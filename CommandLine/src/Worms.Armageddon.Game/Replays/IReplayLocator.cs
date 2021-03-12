using System.Collections.Generic;

namespace Worms.Armageddon.Game.Replays
{
    public interface IReplayLocator
    {
        IReadOnlyCollection<ReplayPaths> GetReplayPaths(string searchPattern);
    }
}
