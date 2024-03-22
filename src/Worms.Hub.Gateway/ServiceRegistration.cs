using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Domain;
using Worms.Hub.Gateway.Domain.Announcers;
using Worms.Hub.Gateway.Domain.Announcers.Slack;
using Worms.Hub.Gateway.Storage.Database;
using Worms.Hub.Gateway.Storage.Files;

namespace Worms.Hub.Gateway;

public static class ServiceRegistration
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection builder) =>
        builder.AddSingleton<IRepository<GameDto>, GamesRepository>()
            .AddSingleton<IRepository<Replay>, ReplaysRepository>()
            .AddSingleton<ISlackAnnouncer, SlackAnnouncer>()
            .AddSingleton<ReplayFileValidator>()
            .AddSingleton<CliFileValidator>()
            .AddSingleton<CliFiles>()
            .AddSingleton<SchemeFiles>();
}
