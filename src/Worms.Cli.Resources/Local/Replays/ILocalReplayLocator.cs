namespace Worms.Cli.Resources.Local.Replays;

internal interface ILocalReplayLocator
{
    IReadOnlyCollection<ReplayPaths> GetReplayPaths(string searchPattern);
}
