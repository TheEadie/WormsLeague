using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Worms.Hub.Storage.Database;

[PublicAPI]
public sealed class DatabaseSchemaVersion(IConfiguration configuration)
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);
    private readonly Lock _lock = new();
    private Version? _cached;
    private DateTime _cachedAt = DateTime.MinValue;

    public async Task<Version?> GetCurrentVersionAsync()
    {
        lock (_lock)
        {
            if (_cached is not null && DateTime.UtcNow - _cachedAt < CacheTtl)
                return _cached;
        }

        var version = await FetchFromDatabase();

        lock (_lock)
        {
            _cached = version;
            _cachedAt = DateTime.UtcNow;
        }

        return version;
    }

    private async Task<Version?> FetchFromDatabase()
    {
        try
        {
            var connectionString = configuration.GetConnectionString("Database");
            await using var connection = new NpgsqlConnection(connectionString);
            var versionString = await connection.QuerySingleOrDefaultAsync<string>(
                "SELECT version FROM flyway_schema_history WHERE success = true AND version IS NOT NULL ORDER BY installed_rank DESC LIMIT 1");
            return versionString is null ? null : Version.Parse(versionString);
        }
        catch (NpgsqlException)
        {
            return null;
        }
    }
}
