using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Gateway.Domain;

namespace Worms.Gateway.Database;

public class ReplaysRepository : IRepository<Replay>
{
    private readonly string _connectionString;

    public ReplaysRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Database");
    }

    public IReadOnlyCollection<Replay> Get()
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var dbObjects = connection.Query<ReplayDb>("SELECT id, name, status, tempfilename FROM replays");
        return dbObjects.Select(x => new Replay(x.Id.ToString(), x.Name, x.Status, x.TempFilename)).ToList();
    }

    public Replay Create(Replay item)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql =
            "INSERT INTO replays (name, status, tempfilename) VALUES (@name, @status, @tempfilename) RETURNING id";

        var parameters = new
        {
            name = item.Name,
            status = item.Status,
            tempfilename = item.Filename
        };

        var created = connection.QuerySingle<string>(sql, parameters);
        return item with { Id = created };
    }

    public void Update(Replay item)
    {
        throw new NotImplementedException();
    }
}

public record ReplayDb(int Id, string Name, string Status, string TempFilename);
