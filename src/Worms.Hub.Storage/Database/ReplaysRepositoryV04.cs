using System.Globalization;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

internal sealed class ReplaysRepositoryV04(IConfiguration configuration) : IReplaysRepository
{
    static ReplaysRepositoryV04() =>
        SqlMapper.AddTypeHandler(StringArrayHandler.Instance);

    public IReadOnlyCollection<Replay> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var dbObjects = connection.Query<ReplayDb>(
            "SELECT id, name, status, filename, fullLog, "
            + "league_id AS LeagueId, date AS Date, winner AS Winner, teams AS Teams "
            + "FROM replays");
        return [.. dbObjects.Select(x => x.ToDomain())];
    }

    public IReadOnlyList<Replay> GetByLeagueId(string leagueId)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "SELECT id, name, status, filename, fullLog, "
            + "league_id AS LeagueId, date AS Date, winner AS Winner, teams AS Teams "
            + "FROM replays WHERE league_id = @leagueId ORDER BY date DESC NULLS LAST";
        var dbObjects = connection.Query<ReplayDb>(sql, new { leagueId });
        return [.. dbObjects.Select(x => x.ToDomain())];
    }

    public Replay Create(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "INSERT INTO replays "
            + "(name, status, filename, fullLog, league_id, date, winner, teams) "
            + "VALUES (@name, @status, @filename, @fullLog, @leagueId, @date, @winner, @teams) "
            + "RETURNING id";
        var parameters = new
        {
            name = item.Name,
            status = item.Status,
            filename = item.Filename,
            fullLog = item.FullLog,
            leagueId = item.LeagueId,
            date = item.Date,
            winner = item.Winner,
            teams = item.Teams?.ToArray()
        };
        var created = connection.QuerySingle<string>(sql, parameters);
        return item with { Id = created };
    }

    public void Update(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "UPDATE replays SET "
            + "name = @name, "
            + "status = @status, "
            + "filename = @filename, "
            + "fullLog = @fullLog, "
            + "league_id = @leagueId, "
            + "date = @date, "
            + "winner = @winner, "
            + "teams = @teams "
            + "WHERE id = @id";
        var parameters = new
        {
            id = int.Parse(item.Id, CultureInfo.InvariantCulture),
            name = item.Name,
            status = item.Status,
            filename = item.Filename,
            fullLog = item.FullLog,
            leagueId = item.LeagueId,
            date = item.Date,
            winner = item.Winner,
            teams = item.Teams?.ToArray()
        };
        _ = connection.Execute(sql, parameters);
    }
}
