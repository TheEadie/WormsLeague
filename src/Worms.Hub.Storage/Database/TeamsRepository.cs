using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class TeamsRepository(IConfiguration configuration) : ITeamsRepository
{
    private const string SelectSql =
        "SELECT t.id AS Id, t.machine AS Machine, t.team_name AS TeamName, "
        + "p.display_name AS ClaimedByPlayerName, p.auth0_subject AS ClaimedByAuth0Subject "
        + "FROM teams t LEFT JOIN players p ON t.player_id = p.id";

    public IReadOnlyCollection<Team> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<TeamDb>(SelectSql).Select(MapToDomain)];
    }

    public Team? GetById(int id)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var db = connection.QuerySingleOrDefault<TeamDb>(SelectSql + " WHERE t.id = @id", new { id });
        return db is null ? null : MapToDomain(db);
    }

    public void Upsert(string machine, string teamName)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        _ = connection.Execute(
            "INSERT INTO teams (machine, team_name) VALUES (@machine, @teamName) "
            + "ON CONFLICT DO NOTHING",
            new { machine, teamName });
    }

    public void SetPlayerClaim(int teamId, int? playerId)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        _ = connection.Execute(
            "UPDATE teams SET player_id = @playerId WHERE id = @teamId",
            new { teamId, playerId });
    }

    private static Team MapToDomain(TeamDb db) =>
        new(db.Id, db.Machine, db.TeamName, db.ClaimedByPlayerName, db.ClaimedByAuth0Subject);
}

[PublicAPI]
public record TeamDb(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedByPlayerName,
    string? ClaimedByAuth0Subject);
