using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Worms.Hub.Queues.Fake;

[PublicAPI]
public static class ServiceRegistration
{
    public static IServiceCollection AddFakeQueueServices(this IServiceCollection services)
    {
        var replaysToProcess = new FakeMessageQueue<ReplayToProcessMessage>();
        services.RemoveAll<IMessageQueue<ReplayToProcessMessage>>();
        services.AddSingleton<IMessageQueue<ReplayToProcessMessage>>(replaysToProcess);
        services.AddSingleton(replaysToProcess);

        var replaysToUpdate = new FakeMessageQueue<ReplayToUpdateMessage>();
        services.RemoveAll<IMessageQueue<ReplayToUpdateMessage>>();
        services.AddSingleton<IMessageQueue<ReplayToUpdateMessage>>(replaysToUpdate);
        services.AddSingleton(replaysToUpdate);

        return services;
    }
}
