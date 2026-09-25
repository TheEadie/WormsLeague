using Worms.Hub.Gateway.Announcers;

namespace Worms.Hub.Gateway.Tests.Worker;

internal sealed record GameCompleteAnnouncement(
    string Winner,
    IReadOnlyList<PlacementInfo>? Placements,
    IReadOnlyList<LeaderboardEntry>? Leaderboard,
    string? LeaderboardFailureNote);

internal sealed class FakeAnnouncer : IAnnouncer
{
    private readonly List<string> _gameStartingAnnouncements = [];
    private readonly List<GameCompleteAnnouncement> _gameCompleteAnnouncements = [];

    public IReadOnlyList<string> GameStartingAnnouncements => _gameStartingAnnouncements;

    public IReadOnlyList<GameCompleteAnnouncement> GameCompleteAnnouncements => _gameCompleteAnnouncements;

    public Task AnnounceGameStarting(string hostName)
    {
        _gameStartingAnnouncements.Add(hostName);
        return Task.CompletedTask;
    }

    public Task AnnounceGameComplete(
        string winner,
        IReadOnlyList<PlacementInfo>? placements = null,
        IReadOnlyList<LeaderboardEntry>? leaderboard = null,
        string? leaderboardFailureNote = null)
    {
        _gameCompleteAnnouncements.Add(
            new GameCompleteAnnouncement(winner, placements, leaderboard, leaderboardFailureNote));
        return Task.CompletedTask;
    }
}
