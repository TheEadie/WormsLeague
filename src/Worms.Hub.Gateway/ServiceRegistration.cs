using Microsoft.Extensions.DependencyInjection.Extensions;
using Worms.Armageddon.Files;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Announcers.Slack;
using Worms.Hub.Gateway.API.Validators;
using Worms.Hub.Gateway.FeatureFlags;
using Worms.Hub.Gateway.Worker;
using Worms.Hub.Queues;
using Worms.Hub.Storage;

namespace Worms.Hub.Gateway;

internal static class ServiceRegistration
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection builder) =>
        builder.AddWormsArmageddonFilesServices().AddHttpClient().AddScoped<IAnnouncer, Announcer>().AddScoped<ReplayFileValidator>().AddScoped<CliFileValidator>().AddScoped<IFeatureFlags, GatewayFeatureFlags>();

    public static IServiceCollection AddWorkerServices(this IServiceCollection builder)
    {
        _ = builder.AddHubStorageServices()
            .AddQueueServices()
            .AddWormsArmageddonFilesServices()
            .AddHttpClient()
            .AddScoped<Processor>()
            .AddScoped<IAnnouncer, Announcer>();
        builder.TryAddScoped<IFeatureFlags, GatewayFeatureFlags>();
        return builder;
    }
}
