using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PlayerRank;
using PlayerRank.Scoring;
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
    private sealed record MultiPlayerSelection(string AuthSubject, string Machine, string TeamName);

    private sealed record MultiPlayerReplayInfo(
        int ReplayId,
        int RecordedGameIndex,
        IReadOnlyList<MultiPlayerSelection> Selections);

    private sealed record SinglePlayerReplayInfo(
        int ReplayId,
        string AuthSubject,
        string Machine,
        string TeamName,
        int PriorRecordedGameIndex);

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
        // games_played counts: playerAuthSubject -> count of replays with at least one claimed placement
        var gamesPlayed = new Dictionary<string, int>();

        // Per-replay bookkeeping for delta write-back.
        var multiPlayerReplays = new List<MultiPlayerReplayInfo>();
        var singlePlayerReplays = new List<SinglePlayerReplayInfo>();
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

            // Count games_played for each matched player
            foreach (var mp in matchedPlayers)
            {
                gamesPlayed.TryAdd(mp.AuthSubject, 0);
                gamesPlayed[mp.AuthSubject]++;
            }

            if (matchedPlayers.Count == 0)
            {
                continue;
            }

            if (matchedPlayers.Count == 1)
            {
                var only = matchedPlayers[0];
                singlePlayerReplays.Add(new SinglePlayerReplayInfo(
                    int.Parse(replay.Id, CultureInfo.InvariantCulture),
                    only.AuthSubject,
                    only.Machine,
                    only.TeamName,
                    recordedGameCount));
                continue;
            }

            // matchedPlayers.Count >= 2 — record the game.
            var game = new PlayerRank.Game();
            foreach (var mp in matchedPlayers)
            {
                game.AddResult(mp.AuthSubject, new Position(mp.Position));
            }

            league.RecordGame(game);
            recordedGameCount++;

            multiPlayerReplays.Add(new MultiPlayerReplayInfo(
                int.Parse(replay.Id, CultureInfo.InvariantCulture),
                recordedGameCount,
                matchedPlayers.ConvertAll(
                    mp => new MultiPlayerSelection(mp.AuthSubject, mp.Machine, mp.TeamName))));
        }

        var eloStrategy = new EloScoringStrategy(new Points(64), new Points(400), new Points(1000));
        var leaderboard = league.GetLeaderBoard(eloStrategy).ToList();

        // Build result: all players with at least one game
        var ratings = gamesPlayed.Keys.Select(authSubject =>
        {
            var score = leaderboard.FirstOrDefault(s => s.Name == authSubject);
            var elo = score is not null ? (int)score.Points.GetValue() : 1000;
            return new PlayerRating(authSubject, string.Empty, leagueId, elo, gamesPlayed[authSubject]);
        }).ToList();

        ratingsRepository.ReplaceForLeague(leagueId, ratings);

        // ---- Per-placement delta write-back ----
        var history = league.GetLeaderBoardHistory(eloStrategy).ToList();

        // 1. Clear every placement in the league.
        replaysRepository.ClearPlacementEloForLeague(leagueId);

        // 2. Write multi-player rows.
        foreach (var r in multiPlayerReplays)
        {
            var post = history[r.RecordedGameIndex];
            var pre = history[r.RecordedGameIndex - 1];
            foreach (var sel in r.Selections)
            {
                var after = RatingAt(post, sel.AuthSubject);
                var before = RatingAt(pre, sel.AuthSubject);
                replaysRepository.UpdatePlacementElo(
                    r.ReplayId,
                    sel.Machine,
                    sel.TeamName,
                    eloDelta: after - before,
                    eloAfter: after);
            }
        }

        // 3. Write single-matched-player rows.
        foreach (var r in singlePlayerReplays)
        {
            var snap = r.PriorRecordedGameIndex == 0 ? null : history[r.PriorRecordedGameIndex];
            var elo = RatingAt(snap, r.AuthSubject);
            replaysRepository.UpdatePlacementElo(
                r.ReplayId,
                r.Machine,
                r.TeamName,
                eloDelta: 0,
                eloAfter: elo);
        }
    }

    private static int RatingAt(History? snapshot, string authSubject)
    {
        if (snapshot is null)
        {
            return 1000;
        }

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
