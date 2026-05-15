namespace Worms.Hub.Gateway.Worker;

internal sealed class PlacementsBackfillService(PlacementsBackfiller backfiller) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        backfiller.RunAsync(stoppingToken);
}
