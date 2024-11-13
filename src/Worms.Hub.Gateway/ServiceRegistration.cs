using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Announcers.Slack;
using Worms.Hub.Gateway.API.Validators;

namespace Worms.Hub.Gateway;

internal static class ServiceRegistration
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection builder) =>
        builder.AddScoped<ISlackAnnouncer, SlackAnnouncer>()
            .AddScoped<ReplayFileValidator>()
            .AddScoped<CliFileValidator>();
}
