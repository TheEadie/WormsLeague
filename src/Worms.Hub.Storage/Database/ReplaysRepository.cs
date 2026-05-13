using System.Data;
using System.Globalization;
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public class ReplaysRepository(IConfiguration configuration) : IRepository<Replay>
{
    static ReplaysRepository() =>
        SqlMapper.AddTypeHandler(new StringArrayHandler());

    private sealed class StringArrayHandler : SqlMapper.TypeHandler<string[]>
    {
        public override void SetValue(IDbDataParameter parameter, string[]? value) =>
            parameter.Value = value ?? (object)DBNull.Value;

        public override string[] Parse(object value) =>
            value as string[] ?? [];
    }

    public virtual IReadOnlyCollection<Replay> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var dbObjects = connection.Query<ReplayDb>("SELECT id, name, status, filename, fullLog FROM replays");
        return [.. dbObjects.Select(MapToDomain)];
    }

    public virtual Replay Create(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql = "INSERT INTO replays (name, status, filename, fullLog) "
            + "VALUES (@name, @status, @filename, @fullLog) RETURNING id";
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

    public virtual void Update(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
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

    protected static Replay MapToDomain(ReplayDb x) =>
        new(
            x.Id.ToString(CultureInfo.InvariantCulture),
            x.Name,
            x.Status,
            x.Filename,
            x.FullLog,
            x.LeagueId,
            x.Date,
            x.Winner,
            x.Teams);
}

#pragma warning disable CA1819 // Dapper requires the concrete array type for constructor-based mapping
[PublicAPI]
public record ReplayDb(
    int Id,
    string Name,
    string Status,
    string Filename,
    string? FullLog,
    string? LeagueId,
    DateTime? Date,
    string? Winner,
    string[]? Teams);
#pragma warning restore CA1819
