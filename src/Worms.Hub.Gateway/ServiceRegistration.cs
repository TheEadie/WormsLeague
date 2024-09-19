using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Domain.Announcers;
using Worms.Hub.Gateway.Domain.Announcers.Slack;

namespace Worms.Hub.Gateway;

public static class ServiceRegistration
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection builder) =>
        builder.AddScoped<ISlackAnnouncer, SlackAnnouncer>()
            .AddScoped<ReplayFileValidator>()
            .AddScoped<CliFileValidator>();
}
