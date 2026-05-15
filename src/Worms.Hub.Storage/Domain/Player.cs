using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Player(int Id, string Auth0Subject, string DisplayName);
