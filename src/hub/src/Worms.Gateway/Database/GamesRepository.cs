using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Database;

public class GamesRepository : IRepository<GameDto>
{
    private readonly string _connectionString;

    public GamesRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Database");
    }
    
    public IReadOnlyCollection<GameDto> Get()
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var dbObjects = connection.Query<GamesDb>("SELECT id, status, hostmachine FROM games");
        return dbObjects.Select(x => new GameDto(x.Id.ToString(), x.Status, x.HostMachine)).ToList();
    }
}

public record GamesDb (int Id, string Status, string HostMachine);