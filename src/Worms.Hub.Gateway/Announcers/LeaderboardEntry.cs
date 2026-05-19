namespace Worms.Hub.Gateway.Announcers;

/// <summary>
/// One row in the post-game leaderboard Slack block.
/// </summary>
/// <param name="Rank">1-based rank number; shared by players with equal ELO.</param>
/// <param name="Elo">Current ELO rating.</param>
/// <param name="DisplayName">Player display name.</param>
/// <param name="EloDelta">
/// Change in ELO from this game. Null if the player did not participate
/// or their rating did not change.
/// </param>
/// <param name="PositionChange">
/// Change in rank position (positive = fell, negative = improved).
/// Null if the rank did not change. Zero is never stored — use null for no change.
/// </param>
internal sealed record LeaderboardEntry(
    int Rank,
    int Elo,
    string DisplayName,
    int? EloDelta,
    int? PositionChange);
