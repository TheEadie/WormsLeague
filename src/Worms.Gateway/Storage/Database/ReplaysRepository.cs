using System.Globalization;
using Dapper;
using Npgsql;
using Worms.Gateway.Domain;

namespace Worms.Gateway.Storage.Database;

internal sealed class ReplaysRepository : IRepository<Replay>
{
    private readonly IConfiguration _configuration;

    public ReplaysRepository(IConfiguration configuration) => _configuration = configuration;

    public IReadOnlyCollection<Replay> GetAll()
    {
        var connectionString = _configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);

        var dbObjects = connection.Query<ReplayDb>("SELECT id, name, status, filename FROM replays");
        return dbObjects
            .Select(x => new Replay(x.Id.ToString(CultureInfo.InvariantCulture), x.Name, x.Status, x.Filename))
            .ToList();
    }

    public Replay Create(Replay item)
    {
        var connectionString = _configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "INSERT INTO replays "
            + "(name, status, filename) "
            + "VALUES (@name, @status, @filename) "
            + "RETURNING id";

        var parameters = new
        {
            name = item.Name,
            status = item.Status,
            filename = item.Filename
        };

        var created = connection.QuerySingle<string>(sql, parameters);
        return item with { Id = created };
    }

    public void Update(Replay item)
    {
        var connectionString = _configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "UPDATE replays SET "
            + "name = @name, "
            + "status = @status, "
            + "filename = @filename "
            + "WHERE id = @id";

        var parameters = new
        {
            id = int.Parse(item.Id, CultureInfo.InvariantCulture),
            name = item.Name,
            status = item.Status,
            filename = item.Filename
        };

        _ = connection.Execute(sql, parameters);
    }
}

public record ReplayDb(int Id, string Name, string Status, string Filename);
