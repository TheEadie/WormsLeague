using System.Globalization;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

internal sealed class GamesRepository(IConfiguration configuration) : IRepository<Game>
{
    public IReadOnlyCollection<Game> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);

        var dbObjects = connection.Query<GamesDb>("SELECT id, status, hostmachine FROM games");
        return dbObjects.Select(x => new Game(x.Id.ToString(CultureInfo.InvariantCulture), x.Status, x.HostMachine))
            .ToList();
    }

    public Game Create(Game item)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "INSERT INTO games (status, hostmachine) VALUES (@status, @hostmachine) RETURNING id";

        var parameters = new
        {
            status = item.Status,
            hostmachine = item.HostMachine
        };

        var created = connection.QuerySingle<string>(sql, parameters);
        return item with { Id = created };
    }

    public void Update(Game item)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "UPDATE games SET status = @status, hostmachine = @hostmachine WHERE id = @id";

        var parameters = new
        {
            id = int.Parse(item.Id, CultureInfo.InvariantCulture),
            status = item.Status,
            hostmachine = item.HostMachine
        };

        _ = connection.Execute(sql, parameters);
    }
}

public record GamesDb(int Id, string Status, string HostMachine);
