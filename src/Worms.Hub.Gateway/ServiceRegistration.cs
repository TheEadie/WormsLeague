using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Announcers.Slack;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.Worker;
using Worms.Hub.Queues;
using Worms.Hub.Storage;

namespace Worms.Hub.Gateway;

internal static class ServiceRegistration
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection builder) =>
        builder.AddScoped<ISlackAnnouncer, SlackAnnouncer>()
            .AddScoped<ReplayFileValidator>()
            .AddScoped<CliFileValidator>();

    public static IServiceCollection AddWorkerServices(this IServiceCollection builder) =>
        builder.AddHubStorageServices().AddQueueServices().AddScoped<Processor>();
}
