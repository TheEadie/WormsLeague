using Worms.Armageddon.Game;
using Worms.Hub.Queues;

namespace Worms.Hub.ReplayProcessor;

internal static class ServiceRegistration
{
    public static IServiceCollection AddReplayProcessorServices(this IServiceCollection builder) =>
        builder.AddWormsArmageddonGameServices().AddQueueServices().AddScoped<Processor>();
}
