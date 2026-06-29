using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Fake;

[PublicAPI]
public static class ServiceRegistration
{
    public static IServiceCollection AddFakeHubStorageServices(this IServiceCollection services)
    {
        var fakes = new FakeHubStorage();

        services.RemoveAll<IRepository<Game>>();
        services.AddSingleton<IRepository<Game>>(_ => fakes.Games);

        services.RemoveAll<IReplaysRepository>();
        services.AddSingleton<IReplaysRepository>(_ => fakes.Replays);

        services.RemoveAll<ILeaguesRepository>();
        services.AddSingleton<ILeaguesRepository>(_ => fakes.Leagues);

        services.RemoveAll<IRatingsRepository>();
        services.AddSingleton<IRatingsRepository>(_ => fakes.Ratings);

        services.RemoveAll<ITeamsRepository>();
        services.AddSingleton<ITeamsRepository>(_ => fakes.Teams);

        services.RemoveAll<IPlayersRepository>();
        services.AddSingleton<IPlayersRepository>(_ => fakes.Players);

        services.AddSingleton(fakes);
        return services;
    }
}
