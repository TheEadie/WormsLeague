using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class PlayersRepository(IConfiguration configuration) : IPlayersRepository
{
    public Player? GetByAuth0Subject(string auth0Subject)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var db = connection.QuerySingleOrDefault<PlayerDb>(
            "SELECT id, auth0_subject AS Auth0Subject, display_name AS DisplayName "
            + "FROM players WHERE auth0_subject = @auth0Subject",
            new { auth0Subject });
        return db is null ? null : new Player(db.Id, db.Auth0Subject, db.DisplayName);
    }

    public Player Create(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql =
            "INSERT INTO players (auth0_subject, display_name) "
            + "VALUES (@auth0Subject, @displayName) RETURNING id";
        var id = connection.QuerySingle<int>(sql,
            new { auth0Subject = player.Auth0Subject, displayName = player.DisplayName });
        return player with { Id = id };
    }
}

[PublicAPI]
public record PlayerDb(int Id, string Auth0Subject, string DisplayName);
