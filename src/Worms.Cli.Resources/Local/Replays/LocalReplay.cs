using Worms.Armageddon.Files.Replays;

namespace Worms.Cli.Resources.Local.Replays;

public record LocalReplay(ReplayPaths Paths, ReplayResource Details, bool HostedByLocalMachine)
    : ReplayWithContext("local");
