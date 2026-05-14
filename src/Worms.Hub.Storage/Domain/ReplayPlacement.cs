using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record ReplayPlacement(string Machine, string TeamName, int Position);
