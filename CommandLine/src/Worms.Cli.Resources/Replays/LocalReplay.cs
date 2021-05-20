using Worms.Armageddon.Resources.Replays;

namespace Worms.Cli.Resources.Replays
{
    public abstract record ReplayWithContext(string Context);

    public record LocalReplay(ReplayPaths Paths, ReplayResource Details) : ReplayWithContext("local");
}
