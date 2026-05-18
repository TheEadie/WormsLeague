namespace Worms.Hub.Gateway.Worker;

internal sealed class StartupBackfillService(StartupBackfiller backfiller) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        backfiller.RunAsync(stoppingToken);
}
