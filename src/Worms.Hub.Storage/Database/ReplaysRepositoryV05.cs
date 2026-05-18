using System.Globalization;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

internal sealed class ReplaysRepositoryV05(IConfiguration configuration) : IReplaysRepository
{
    private sealed record ReplayPlacementDb(int ReplayId, string Machine, string TeamName, int? Position, string? PlayerName)
    {
        public ReplayPlacement ToDomain() => new(Machine, TeamName, Position, PlayerName);
    }

    static ReplaysRepositoryV05() =>
        SqlMapper.AddTypeHandler(StringArrayHandler.Instance);

    public IReadOnlyCollection<Replay> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var dbObjects = connection.Query<ReplayDb>(
            "SELECT id, name, status, filename, fullLog, "
            + "league_id AS LeagueId, date AS Date, winner AS Winner, teams AS Teams "
            + "FROM replays").ToList();
        var ids = dbObjects.Select(r => r.Id).ToArray();
        var placements = ids.Length > 0
            ? connection.Query<ReplayPlacementDb>(
                    "SELECT rp.replay_id AS ReplayId, rp.machine AS Machine, rp.team_name AS TeamName, "
                    + "rp.position AS Position, pl.display_name AS PlayerName "
                    + "FROM replay_placements rp "
                    + "LEFT JOIN teams t ON t.machine = rp.machine AND t.team_name = rp.team_name "
                    + "LEFT JOIN players pl ON pl.auth_subject = t.player_auth_subject "
                    + "WHERE rp.replay_id = ANY(@ids)",
                    new { ids })
                .ToLookup(p => p.ReplayId)
            : Enumerable.Empty<ReplayPlacementDb>().ToLookup(p => p.ReplayId);
        return [.. dbObjects.Select(r => r.ToDomain() with
        {
            Placements = placements[r.Id].Select(p => p.ToDomain()).ToList()
        })];
    }

    public IReadOnlyList<Replay> GetByLeagueId(string leagueId)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "SELECT id, name, status, filename, fullLog, "
            + "league_id AS LeagueId, date AS Date, winner AS Winner, teams AS Teams "
            + "FROM replays WHERE league_id = @leagueId ORDER BY date DESC NULLS LAST";
        var dbObjects = connection.Query<ReplayDb>(sql, new { leagueId }).ToList();
        var ids = dbObjects.Select(r => r.Id).ToArray();
        var placements = ids.Length > 0
            ? connection.Query<ReplayPlacementDb>(
                    "SELECT rp.replay_id AS ReplayId, rp.machine AS Machine, rp.team_name AS TeamName, "
                    + "rp.position AS Position, pl.display_name AS PlayerName "
                    + "FROM replay_placements rp "
                    + "LEFT JOIN teams t ON t.machine = rp.machine AND t.team_name = rp.team_name "
                    + "LEFT JOIN players pl ON pl.auth_subject = t.player_auth_subject "
                    + "WHERE rp.replay_id = ANY(@ids)",
                    new { ids })
                .ToLookup(p => p.ReplayId)
            : Enumerable.Empty<ReplayPlacementDb>().ToLookup(p => p.ReplayId);
        return [.. dbObjects.Select(r => r.ToDomain() with
        {
            Placements = placements[r.Id].Select(p => p.ToDomain()).ToList()
        })];
    }

    public Replay Create(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

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
        var replayId = connection.QuerySingle<int>(sql, parameters, transaction);

        if (item.Placements is { Count: > 0 })
        {
            foreach (var p in item.Placements)
            {
                _ = connection.Execute(
                    "INSERT INTO replay_placements (replay_id, machine, team_name, position) "
                    + "VALUES (@replayId, @machine, @teamName, @position)",
                    new { replayId, machine = p.Machine, teamName = p.TeamName, position = p.Position },
                    transaction);
            }
        }

        transaction.Commit();
        return item with { Id = replayId.ToString(CultureInfo.InvariantCulture) };
    }

    public void Update(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var replayId = int.Parse(item.Id, CultureInfo.InvariantCulture);

        const string replaySql = "UPDATE replays SET "
            + "name = @name, status = @status, filename = @filename, fullLog = @fullLog, "
            + "league_id = @leagueId, date = @date, winner = @winner, teams = @teams "
            + "WHERE id = @replayId";
        _ = connection.Execute(replaySql, new
        {
            replayId,
            name = item.Name,
            status = item.Status,
            filename = item.Filename,
            fullLog = item.FullLog,
            leagueId = item.LeagueId,
            date = item.Date,
            winner = item.Winner,
            teams = item.Teams?.ToArray()
        }, transaction);

        _ = connection.Execute(
            "DELETE FROM replay_placements WHERE replay_id = @replayId",
            new { replayId },
            transaction);

        if (item.Placements is { Count: > 0 })
        {
            foreach (var p in item.Placements)
            {
                _ = connection.Execute(
                    "INSERT INTO replay_placements (replay_id, machine, team_name, position) "
                    + "VALUES (@replayId, @machine, @teamName, @position)",
                    new { replayId, machine = p.Machine, teamName = p.TeamName, position = p.Position },
                    transaction);
            }
        }

        transaction.Commit();
    }
}
