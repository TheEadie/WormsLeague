using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Team(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedByPlayerName,
    string? ClaimedByAuthSubject);
