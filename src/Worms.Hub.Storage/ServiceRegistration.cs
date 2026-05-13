using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using JetBrains.Annotations;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Storage;

[PublicAPI]
public static class ServiceRegistration
{
    private static readonly Version ReplayLeagueFieldsMinVersion = new(0, 4);

    public static IServiceCollection AddHubStorageServices(this IServiceCollection builder) =>
        builder.AddSingleton<DatabaseSchemaVersion>()
            .AddScoped<IRepository<Game>, GamesRepository>()
            .AddScoped<ReplaysRepositoryV04>()
            .AddScoped<IReplaysRepositoryV04>(sp => sp.GetRequiredService<ReplaysRepositoryV04>())
            .AddScoped<IRepository<Replay>>(sp =>
            {
                var version = sp.GetRequiredService<DatabaseSchemaVersion>()
                    .GetCurrentVersionAsync().GetAwaiter().GetResult();
                return version is not null && version >= ReplayLeagueFieldsMinVersion
                    ? sp.GetRequiredService<ReplaysRepositoryV04>()
                    : new ReplaysRepository(sp.GetRequiredService<IConfiguration>());
            })
            .AddScoped<LeaguesRepository>()
            .AddScoped<CliFiles>()
            .AddScoped<ReplayFiles>()
            .AddScoped<SchemeFiles>()
            .AddScoped<GameFiles>();
}
