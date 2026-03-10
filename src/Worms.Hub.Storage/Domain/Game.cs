using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public record Game(string Id, string Status, string HostMachine);
