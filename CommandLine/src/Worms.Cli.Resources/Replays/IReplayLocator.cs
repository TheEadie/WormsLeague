using System.Collections.Generic;

namespace Worms.Cli.Resources.Replays
{
    public interface IReplayLocator
    {
        IReadOnlyCollection<ReplayPaths> GetReplayPaths(string searchPattern);
    }
}
