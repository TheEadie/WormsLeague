using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Armageddon.Gifs;
using Worms.Hub.Queues;

namespace Worms.Hub.Armageddon.Runner;

internal static class ServiceRegistration
{
    public static IServiceCollection AddReplayProcessorServices(this IServiceCollection builder) =>
        builder.AddWormsArmageddonGameServices()
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGifsServices()
            .AddQueueServices()
            .AddScoped<Processor>();
}
