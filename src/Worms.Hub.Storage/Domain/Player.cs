using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Player(int Id, string AuthSubject, string DisplayName);
