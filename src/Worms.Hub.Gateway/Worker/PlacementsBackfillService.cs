using System.Diagnostics.CodeAnalysis;
using Dapper;
using Npgsql;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.FeatureFlags;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Worker;

internal sealed class PlacementsBackfillService(
    IServiceProvider serviceProvider,
    ILogger<PlacementsBackfillService> logger) : BackgroundService
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Backfill continues with remaining replays even if one fails")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = Telemetry.Source.StartActivity("Placement Backfill");

        using var scope = serviceProvider.CreateScope();
        var featureFlags = scope.ServiceProvider.GetRequiredService<IFeatureFlags>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var replayRepository = scope.ServiceProvider.GetRequiredService<IReplaysRepository>();
        var replayTextReader = scope.ServiceProvider.GetRequiredService<IReplayTextReader>();

        if (!await featureFlags.IsPlacementsEnabledAsync())
        {
            logger.LogInformation("Placements feature not enabled — skipping backfill.");
            return;
        }

        var connectionString = configuration.GetConnectionString("Database");
        await using var connection = new NpgsqlConnection(connectionString);

        if (await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM replay_placements") > 0)
        {
            logger.LogInformation("Placement backfill already complete — skipping.");
            return;
        }

        logger.LogInformation("Starting placement backfill...");

        foreach (var replay in replayRepository.GetAll().Where(r => r.Status == "Processed"))
        {
            if (string.IsNullOrEmpty(replay.FullLog))
            {
                logger.LogDebug("Skipping replay {ReplayId} — no full log available.", replay.Id);
                continue;
            }

            try
            {
                var replayModel = replayTextReader.GetModel(replay.FullLog);
                var placements = replayModel.Placements
                    .Select(p => new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position))
                    .ToList();
                replayRepository.Update(replay with { Placements = placements });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to backfill placements for replay {ReplayId}.", replay.Id);
            }
        }

        logger.LogInformation("Placement backfill complete.");
    }
}
