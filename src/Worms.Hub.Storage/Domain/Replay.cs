using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Replay(string Id, string Name, string Status, string Filename, string? FullLog);
