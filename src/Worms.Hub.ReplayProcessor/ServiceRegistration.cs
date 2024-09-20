using Microsoft.Extensions.DependencyInjection;

namespace Worms.Hub.ReplayProcessor;

public static class ServiceRegistration
{
    public static IServiceCollection AddReplayProcessorServices(this IServiceCollection builder) =>
        builder.AddScoped<Processor>();
}
