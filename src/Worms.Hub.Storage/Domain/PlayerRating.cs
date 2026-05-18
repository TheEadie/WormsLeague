using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record PlayerRating(
    string PlayerAuthSubject,
    string DisplayName,
    string LeagueId,
    int Rating,
    int GamesPlayed);
