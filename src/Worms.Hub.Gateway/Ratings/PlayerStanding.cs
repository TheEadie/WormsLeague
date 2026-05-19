namespace Worms.Hub.Gateway.Ratings;

internal sealed record PlayerStanding(
    string PlayerAuthSubject,
    string DisplayName,
    int Rank,
    int Rating);
