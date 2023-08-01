using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Database;

public class GamesRepository : IRepository<GameDto>
{
    private readonly IConfiguration _configuration;

    public GamesRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IReadOnlyCollection<GameDto> Get()
    {
        var connectionString = _configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);

        var dbObjects = connection.Query<GamesDb>("SELECT id, status, hostmachine FROM games");
        return dbObjects.Select(x => new GameDto(x.Id.ToString(), x.Status, x.HostMachine)).ToList();
    }

    public GameDto Create(GameDto item)
    {
        var connectionString = _configuration.GetConnectionString("Database");
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

    public void Update(GameDto item)
    {
        var connectionString = _configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "UPDATE games SET status = @status, hostmachine = @hostmachine WHERE id = @id";

        var parameters = new
        {
            id = int.Parse(item.Id),
            status = item.Status,
            hostmachine = item.HostMachine
        };

        connection.Execute(sql, parameters);
    }
}

public record GamesDb(int Id, string Status, string HostMachine);
