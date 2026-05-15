using JetBrains.Annotations;

namespace Worms.Armageddon.Files.Replays;

[PublicAPI]
public record Placement(Team Team, int? Position);
