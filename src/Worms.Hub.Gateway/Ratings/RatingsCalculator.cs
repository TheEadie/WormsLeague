using PlayerRank;
using PlayerRank.Scoring.Elo;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Ratings;

internal sealed class RatingsCalculator(
    IReplaysRepository replaysRepository,
    ITeamsRepository teamsRepository,
    IRatingsRepository ratingsRepository)
{
    public void Calculate(string leagueId)
    {
        // Build alias lookup: (machine, teamName) -> playerAuthSubject
        var claimedTeams = teamsRepository.GetAll()
            .Where(t => t.ClaimedByAuthSubject is not null)
            .ToDictionary(t => (t.Machine, t.TeamName), t => t.ClaimedByAuthSubject!);

        // Replays ordered by date ASC, then name ASC as tiebreaker (nulls last)
        var replays = replaysRepository.GetByLeagueId(leagueId)
            .Where(r => r.Status == "Processed" && r.Placements is { Count: > 0 })
            .OrderBy(r => r.Date ?? DateTime.MaxValue)
            .ThenBy(r => r.Name)
            .ToList();

        var league = new PlayerRank.League();
        // games_played counts: playerAuthSubject -> count of replays with at least one claimed placement
        var gamesPlayed = new Dictionary<string, int>();

        foreach (var replay in replays)
        {
            // Map placements to claimed players (may have duplicates if same player has multiple teams in one game — take first)
            var matchedPlayers = replay.Placements!
                .Where(p => p.Position.HasValue && claimedTeams.ContainsKey((p.Machine, p.TeamName)))
                .Select(p => (AuthSubject: claimedTeams[(p.Machine, p.TeamName)], Position: p.Position!.Value))
                .GroupBy(x => x.AuthSubject)
                .Select(g => g.OrderBy(x => x.Position).First()) // best position if same player has multiple teams
                .ToList();

            // Count games_played for each matched player
            foreach (var mp in matchedPlayers)
            {
                gamesPlayed.TryAdd(mp.AuthSubject, 0);
                gamesPlayed[mp.AuthSubject]++;
            }

            // ELO: only include replays with 2+ distinct matched players
            if (matchedPlayers.Count < 2)
            {
                continue;
            }

            var game = new PlayerRank.Game();
            foreach (var mp in matchedPlayers)
            {
                game.AddResult(mp.AuthSubject, new Position(mp.Position));
            }

            league.RecordGame(game);
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
    }
}
