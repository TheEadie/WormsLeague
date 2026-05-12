using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Worms.Hub.Storage.Database;

[PublicAPI]
public sealed class LeaguesRepository(IConfiguration configuration)
{
    public IReadOnlyList<LeagueDb> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<LeagueDb>("SELECT id, name FROM leagues")];
    }

    public LeagueDb? GetById(string id)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return connection.QuerySingleOrDefault<LeagueDb>(
            "SELECT id, name FROM leagues WHERE id = @id", new { id });
    }
}

[PublicAPI]
public record LeagueDb(string Id, string Name);
