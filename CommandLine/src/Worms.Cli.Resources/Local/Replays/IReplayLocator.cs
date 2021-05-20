using System.Collections.Generic;

namespace Worms.Cli.Resources.Local.Replays
{
    public interface IReplayLocator
    {
        IReadOnlyCollection<ReplayPaths> GetReplayPaths(string searchPattern);
    }
}
