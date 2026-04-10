using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public record League(string Id, string Name, Version Version, string SchemePath);
