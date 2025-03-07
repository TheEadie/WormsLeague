using System.Globalization;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

internal sealed class ReplaysRepository(IConfiguration configuration) : IRepository<Replay>
{
    public IReadOnlyCollection<Replay> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);

        var dbObjects = connection.Query<ReplayDb>("SELECT id, name, status, filename, fullLog FROM replays");
        return
        [
            .. dbObjects.Select(
                x => new Replay(
                    x.Id.ToString(CultureInfo.InvariantCulture),
                    x.Name,
                    x.Status,
                    x.Filename,
                    x.FullLog))
        ];
    }

    public Replay Create(Replay item)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "INSERT INTO replays "
            + "(name, status, filename, fullLog) "
            + "VALUES (@name, @status, @filename, @fullLog) "
            + "RETURNING id";

        var parameters = new
        {
            name = item.Name,
            status = item.Status,
            filename = item.Filename,
            fullLog = item.FullLog
        };

        var created = connection.QuerySingle<string>(sql, parameters);
        return item with { Id = created };
    }

    public void Update(Replay item)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "UPDATE replays SET "
            + "name = @name, "
            + "status = @status, "
            + "filename = @filename, "
            + "fullLog = @fullLog "
            + "WHERE id = @id";

        var parameters = new
        {
            id = int.Parse(item.Id, CultureInfo.InvariantCulture),
            name = item.Name,
            status = item.Status,
            filename = item.Filename,
            fullLog = item.FullLog
        };

        _ = connection.Execute(sql, parameters);
    }
}

public record ReplayDb(int Id, string Name, string Status, string Filename, string FullLog);
