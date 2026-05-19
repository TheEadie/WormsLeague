namespace Worms.Hub.Gateway.Announcers;

internal sealed record PlacementInfo(string TeamName, int Position);

internal interface IAnnouncer
{
    Task AnnounceGameStarting(string hostName);

    Task AnnounceGameComplete(
        string winner,
        IReadOnlyList<PlacementInfo>? placements = null,
        IReadOnlyList<LeaderboardEntry>? leaderboard = null,
        string? leaderboardFailureNote = null);
}
