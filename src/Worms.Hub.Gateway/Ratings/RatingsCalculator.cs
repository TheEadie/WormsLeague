using System.Diagnostics.CodeAnalysis;
using PlayerRank;
using PlayerRank.Scoring.Elo;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Ratings;

internal sealed class RatingsCalculator(
    ILeaguesRepository leaguesRepository,
    IReplaysRepository replaysRepository,
    ITeamsRepository teamsRepository,
    IRatingsRepository ratingsRepository,
    ILogger<RatingsCalculator> logger)
{
    private sealed record Selection(string AuthSubject, string Machine, string TeamName);

    private sealed record ReplayInfo(
        Replay Replay,
        int? RecordedGameIndex,
        IReadOnlyList<Selection> Selections);

    public void Calculate(string leagueId)
    {
        // Build alias lookup: (machine, teamName) -> playerAuthSubject
        var claimedTeams = teamsRepository.GetAll()
            .Where(t => t.ClaimedByAuthSubject is not null)
            .ToDictionary(t => (t.Machine, t.TeamName), t => t.ClaimedByAuthSubject!);

        // Replays ordered by date ASC, then name ASC as tiebreaker (nulls last)
        var replays = replaysRepository.GetByLeagueId(leagueId)
            .Where(r => r is { Status: "Processed", Placements.Count: > 0 })
            .OrderBy(r => r.Date ?? DateTime.MaxValue)
            .ThenBy(r => r.Name)
            .ToList();

        var league = new PlayerRank.League();
        var gamesPlayed = new Dictionary<string, int>();
        var visitedReplays = new List<ReplayInfo>();
        var recordedGameCount = 0;

        foreach (var replay in replays)
        {
            // Map placements to claimed players (may have duplicates if same player has multiple teams in one game — take first)
            var matchedPlayers = replay.Placements!
                .Where(p => p.Position.HasValue && claimedTeams.ContainsKey((p.Machine, p.TeamName)))
                .Select(p => (
                    AuthSubject: claimedTeams[(p.Machine, p.TeamName)],
                    Position: p.Position!.Value,
                    p.Machine,
                    p.TeamName))
                .GroupBy(x => x.AuthSubject)
                .Select(g => g.OrderBy(x => x.Position).First()) // best position if same player has multiple teams
                .ToList();

            if (matchedPlayers.Count == 0)
            {
                visitedReplays.Add(new ReplayInfo(replay, RecordedGameIndex: null, Selections: []));
                continue;
            }

            foreach (var mp in matchedPlayers)
            {
                gamesPlayed.TryAdd(mp.AuthSubject, 0);
                gamesPlayed[mp.AuthSubject]++;
            }

            // Solo games are a no-op inside EloScoringStrategy (pairwise loop never fires), so recording
            // them keeps history indexing uniform without affecting ratings.
            var game = new PlayerRank.Game();
            foreach (var mp in matchedPlayers)
            {
                game.AddResult(mp.AuthSubject, new Position(mp.Position));
            }

            league.RecordGame(game);
            recordedGameCount++;

            visitedReplays.Add(new ReplayInfo(
                replay,
                recordedGameCount,
                matchedPlayers.ConvertAll(
                    mp => new Selection(mp.AuthSubject, mp.Machine, mp.TeamName))));
        }

        var eloStrategy = new EloScoringStrategy(new Points(64), new Points(400), new Points(1000));
        var leaderboard = league.GetLeaderBoard(eloStrategy).ToList();

        var ratings = gamesPlayed.Keys.Select(authSubject =>
        {
            var score = leaderboard.FirstOrDefault(s => s.Name == authSubject);
            var elo = score is not null ? (int)score.Points.GetValue() : 1000;
            return new PlayerRating(authSubject, string.Empty, leagueId, elo, gamesPlayed[authSubject]);
        }).ToList();

        ratingsRepository.ReplaceForLeague(leagueId, ratings);

        var history = league.GetLeaderBoardHistory(eloStrategy).ToList();

        foreach (var info in visitedReplays)
        {
            var deltas = info.RecordedGameIndex is { } idx
                ? BuildPlacementDeltas(info.Selections, history[idx - 1], history[idx])
                : [];

            var updatedPlacements = info.Replay.Placements!
                .Select(p => deltas.TryGetValue((p.Machine, p.TeamName), out var d)
                    ? p with { EloDelta = d.Delta, EloAfter = d.After }
                    : p with { EloDelta = null, EloAfter = null })
                .ToList();

            replaysRepository.Update(info.Replay with { Placements = updatedPlacements });
        }
    }

    private static Dictionary<(string Machine, string TeamName), (int Delta, int After)> BuildPlacementDeltas(
        IReadOnlyList<Selection> selections,
        History pre,
        History post) =>
        selections.ToDictionary(
            s => (s.Machine, s.TeamName),
            s =>
            {
                var after = RatingAt(post, s.AuthSubject);
                var before = RatingAt(pre, s.AuthSubject);
                return (Delta: after - before, After: after);
            });

    private static int RatingAt(History snapshot, string authSubject)
    {
        var score = snapshot.Leaderboard.FirstOrDefault(s => s.Name == authSubject);
        return score is null ? 1000 : (int)score.Points.GetValue();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "ELO calculation failure for one league must not block remaining leagues or the claim/unclaim operation")]
    public void CalculateForTeam(string machine, string teamName)
    {
        foreach (var leagueId in leaguesRepository.GetLeaguesTeamPlaysIn(machine, teamName))
        {
            try
            {
                Calculate(leagueId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", leagueId);
            }
        }
    }
}
