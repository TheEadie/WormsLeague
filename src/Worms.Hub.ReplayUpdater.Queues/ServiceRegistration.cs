using Microsoft.Extensions.DependencyInjection;
using Worms.Hub.Queues;

namespace Worms.Hub.ReplayProcessor.Queue;

public static class ServiceRegistration
{
    public static IServiceCollection AddReplayToUpdateQueueServices(this IServiceCollection builder) =>
        builder.AddScoped<IMessageQueue<ReplayToUpdateMessage>, ReplaysToUpdate>();
}
