using System.Diagnostics.CodeAnalysis;
using Dapper;
using Npgsql;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.FeatureFlags;
using Worms.Hub.Gateway.Ratings;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Worker;

internal sealed class StartupBackfiller(
    IServiceProvider serviceProvider,
    ILogger<StartupBackfiller> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await BackfillPlacements(cancellationToken);
        await BackfillRatings(cancellationToken);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Backfill continues with remaining replays even if one fails")]
    private async Task BackfillPlacements(CancellationToken _)
    {
        using var activity = Telemetry.Source.StartActivity("Placement Backfill");

        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var replayRepository = scope.ServiceProvider.GetRequiredService<IReplaysRepository>();
        var replayTextReader = scope.ServiceProvider.GetRequiredService<IReplayTextReader>();

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
                    .Select(p => new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position, null))
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Backfill continues with remaining leagues even if one fails")]
    private async Task BackfillRatings(CancellationToken _)
    {
        using var activity = Telemetry.Source.StartActivity("Ratings Backfill");

        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var featureFlags = scope.ServiceProvider.GetRequiredService<IFeatureFlags>();

        if (!await featureFlags.IsEloRatingsEnabledAsync())
        {
            logger.LogInformation("ELO ratings feature not yet enabled — skipping ratings backfill.");
            return;
        }

        var connectionString = configuration.GetConnectionString("Database");
        await using var connection = new NpgsqlConnection(connectionString);

        if (await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM player_ratings") > 0)
        {
            logger.LogInformation("Ratings backfill already complete — skipping.");
            return;
        }

        logger.LogInformation("Starting ratings backfill...");

        var leaguesRepository = scope.ServiceProvider.GetRequiredService<LeaguesRepository>();
        var ratingsCalculator = scope.ServiceProvider.GetRequiredService<RatingsCalculator>();

        foreach (var league in leaguesRepository.GetAll())
        {
            try
            {
                ratingsCalculator.Calculate(league.Id);
                logger.LogInformation("Calculated ELO ratings for league {LeagueId}.", league.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", league.Id);
            }
        }

        logger.LogInformation("Ratings backfill complete.");
    }
}
