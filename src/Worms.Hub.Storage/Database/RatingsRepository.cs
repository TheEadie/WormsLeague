using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class RatingsRepository(IConfiguration configuration) : IRatingsRepository
{
    public IReadOnlyList<PlayerRating> GetByLeagueId(string leagueId)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<RatingsDb>(
            "SELECT pr.player_auth_subject AS PlayerAuthSubject, p.display_name AS DisplayName, "
            + "pr.league_id AS LeagueId, pr.rating AS Rating, pr.games_played AS GamesPlayed "
            + "FROM player_ratings pr "
            + "JOIN players p ON p.auth_subject = pr.player_auth_subject "
            + "WHERE pr.league_id = @leagueId "
            + "ORDER BY pr.rating DESC",
            new { leagueId }).Select(r => new PlayerRating(r.PlayerAuthSubject, r.DisplayName, r.LeagueId, r.Rating, r.GamesPlayed))];
    }

    public void ReplaceForLeague(string leagueId, IReadOnlyList<PlayerRating> ratings)
    {
        ArgumentNullException.ThrowIfNull(ratings);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        _ = connection.Execute(
            "DELETE FROM player_ratings WHERE league_id = @leagueId",
            new { leagueId },
            transaction);
        foreach (var r in ratings)
        {
            _ = connection.Execute(
                "INSERT INTO player_ratings (player_auth_subject, league_id, rating, games_played) "
                + "VALUES (@playerAuthSubject, @leagueId, @rating, @gamesPlayed)",
                new { playerAuthSubject = r.PlayerAuthSubject, leagueId = r.LeagueId, rating = r.Rating, gamesPlayed = r.GamesPlayed },
                transaction);
        }
        transaction.Commit();
    }
}

[PublicAPI]
internal sealed record RatingsDb(
    string PlayerAuthSubject,
    string DisplayName,
    string LeagueId,
    int Rating,
    int GamesPlayed);
