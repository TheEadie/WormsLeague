using Worms.Hub.Queues;
using Worms.Hub.ReplayProcessor.Queue;

namespace Worms.Hub.ReplayProcessor;

internal sealed class CheckForMessagesService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var messageQueue = scope.ServiceProvider.GetRequiredService<IMessageQueue<ReplayToProcessMessage>>();
        var processor = scope.ServiceProvider.GetRequiredService<Processor>();

        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (await messageQueue.HasPendingMessage())
            {
                await processor.ProcessReplay();
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
