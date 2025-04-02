using Worms.Hub.ReplayProcessor.Queue;
using Worms.Hub.Storage;

namespace Worms.Hub.ReplayUpdater;

internal static class ServiceRegistration
{
    public static IServiceCollection AddReplayUpdaterServices(this IServiceCollection builder) =>
        builder.AddHubStorageServices().AddReplayToUpdateQueueServices().AddScoped<Processor>();
}
