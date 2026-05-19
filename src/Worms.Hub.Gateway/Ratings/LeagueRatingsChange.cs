namespace Worms.Hub.Gateway.Ratings;

internal sealed record LeagueRatingsChange(
    IReadOnlyList<PlayerStanding> Before,
    IReadOnlyList<PlayerStanding> After);
