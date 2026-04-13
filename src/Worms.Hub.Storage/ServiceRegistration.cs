using Microsoft.Extensions.DependencyInjection;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using JetBrains.Annotations;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Storage;

[PublicAPI]
public static class ServiceRegistration
{
    public static IServiceCollection AddHubStorageServices(this IServiceCollection builder) =>
        builder.AddScoped<IRepository<Game>, GamesRepository>()
            .AddScoped<IRepository<Replay>, ReplaysRepository>()
            .AddScoped<CliFiles>()
            .AddScoped<ReplayFiles>()
            .AddScoped<SchemeFiles>()
            .AddScoped<GameFiles>();
}
