using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Worms.Hub.Storage.Database;

[PublicAPI]
public sealed class LeaguesRepository(IConfiguration configuration) : ILeaguesRepository
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

    public IReadOnlyList<string> GetLeaguesTeamPlaysIn(string machine, string teamName)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<string>(
            "SELECT DISTINCT r.league_id "
            + "FROM replay_placements rp "
            + "JOIN replays r ON r.id = rp.replay_id "
            + "WHERE rp.machine = @machine AND rp.team_name = @teamName "
            + "AND r.status = 'Processed' "
            + "AND r.league_id IS NOT NULL",
            new { machine, teamName })];
    }
}

[PublicAPI]
public record LeagueDb(string Id, string Name);
