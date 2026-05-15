using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class PlayersRepository(IConfiguration configuration) : IPlayersRepository
{
    public Player? GetByAuthSubject(string authSubject)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var db = connection.QuerySingleOrDefault<PlayerDb>(
            "SELECT id, auth_subject AS AuthSubject, display_name AS DisplayName "
            + "FROM players WHERE auth_subject = @authSubject",
            new { authSubject });
        return db is null ? null : new Player(db.Id, db.AuthSubject, db.DisplayName);
    }

    public Player Create(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql =
            "INSERT INTO players (auth_subject, display_name) "
            + "VALUES (@authSubject, @displayName) RETURNING id";
        var id = connection.QuerySingle<int>(sql,
            new { authSubject = player.AuthSubject, displayName = player.DisplayName });
        return player with { Id = id };
    }
}

[PublicAPI]
public record PlayerDb(int Id, string AuthSubject, string DisplayName);
