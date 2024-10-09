using Worms.Armageddon.Game;
using Worms.Hub.Storage;

namespace Worms.Hub.ReplayProcessor;

public static class ServiceRegistration
{
    public static IServiceCollection AddReplayProcessorServices(this IServiceCollection builder) =>
        builder.AddHubStorageServices().AddWormsArmageddonGameServices().AddScoped<Processor>();
}
