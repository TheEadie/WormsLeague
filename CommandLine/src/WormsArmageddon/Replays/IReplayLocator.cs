using System.Collections.Generic;

namespace Worms.WormsArmageddon.Replays
{
    public interface IReplayLocator
    {
        IReadOnlyCollection<string> GetReplayPaths(string searchPattern);
    }
}
