using System.Collections.Generic;

namespace Worms.WormsArmageddon.Replays
{
    public interface IReplayLocator
    {
        IReadOnlyCollection<ReplayPaths> GetReplayPaths(string searchPattern);
    }
}
