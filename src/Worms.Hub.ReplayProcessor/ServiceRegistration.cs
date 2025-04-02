using Worms.Armageddon.Game;
using Worms.Hub.ReplayProcessor.Queue;

namespace Worms.Hub.ReplayProcessor;

internal static class ServiceRegistration
{
    public static IServiceCollection AddReplayProcessorServices(this IServiceCollection builder) =>
        builder.AddWormsArmageddonGameServices().AddReplayToProcessQueueServices().AddScoped<Processor>();
}
