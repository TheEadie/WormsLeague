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
        builder.AddScoped<IRepository<GameDto>, GamesRepository>()
            .AddScoped<IRepository<Replay>, ReplaysRepository>()
            .AddScoped<ISlackAnnouncer, SlackAnnouncer>()
            .AddScoped<ReplayFileValidator>()
            .AddScoped<CliFileValidator>()
            .AddScoped<CliFiles>()
            .AddScoped<SchemeFiles>();
}
