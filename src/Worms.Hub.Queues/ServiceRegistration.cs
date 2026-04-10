using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Worms.Hub.Queues;

[PublicAPI]
public static class ServiceRegistration
{
    public static IServiceCollection AddQueueServices(this IServiceCollection builder) =>
        builder.AddScoped<IMessageQueue<ReplayToProcessMessage>, ReplaysToProcess>()
            .AddScoped<IMessageQueue<ReplayToUpdateMessage>, ReplaysToUpdate>();
}
