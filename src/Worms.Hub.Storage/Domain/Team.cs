using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Team(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedByPlayerName,
    string? ClaimedByAuthSubject)
{
    public bool IsClaimedBy(string? subject) =>
        ClaimedByAuthSubject is not null && ClaimedByAuthSubject == subject;

    public bool IsClaimedByAnother(string? subject) =>
        ClaimedByAuthSubject is not null && !IsClaimedBy(subject);
}
