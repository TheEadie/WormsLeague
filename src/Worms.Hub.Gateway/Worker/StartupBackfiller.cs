using System.Diagnostics.CodeAnalysis;
using Dapper;
using Npgsql;
using Worms.Armageddon.Files.Replays.Text;
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
        await RerankAllGames(cancellationToken);
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
                    .Select(p => new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position, null, null, null))
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

        var connectionString = configuration.GetConnectionString("Database");
        await using var connection = new NpgsqlConnection(connectionString);

        var leaguesRepository = scope.ServiceProvider.GetRequiredService<ILeaguesRepository>();
        var ratingsCalculator = scope.ServiceProvider.GetRequiredService<IRatingsCalculator>();

        var ratingsCount = await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM player_ratings");

        List<string> leaguesNeedingRecalc;
        if (ratingsCount == 0)
        {
            // Fresh install — run all leagues.
            leaguesNeedingRecalc = leaguesRepository.GetAll().Select(l => l.Id).ToList();
        }
        else
        {
            // Slice-9 detection: leagues with a placement that *should* carry delta data but doesn't.
            leaguesNeedingRecalc = (await connection.QueryAsync<string>(
                "SELECT DISTINCT league_id FROM ("
                + "SELECT r.league_id "
                + "FROM replay_placements rp "
                + "JOIN replays r ON r.id = rp.replay_id "
                + "JOIN teams t ON t.machine = rp.machine AND t.team_name = rp.team_name "
                + "JOIN replay_placements rp2 ON rp2.replay_id = rp.replay_id "
                + "JOIN teams t2 ON t2.machine = rp2.machine AND t2.team_name = rp2.team_name "
                + "WHERE rp.elo_after IS NULL "
                + "AND rp.position IS NOT NULL "
                + "AND t.player_auth_subject IS NOT NULL "
                + "AND r.status = 'Processed' "
                + "AND r.league_id IS NOT NULL "
                + "AND t2.player_auth_subject IS NOT NULL "
                + "AND rp2.position IS NOT NULL "
                + "GROUP BY r.league_id, rp.replay_id, rp.machine, rp.team_name "
                + "HAVING COUNT(DISTINCT t2.player_auth_subject) >= 2"
                + ") AS placements_needing_delta"))
                .ToList();
        }

        if (leaguesNeedingRecalc.Count == 0)
        {
            logger.LogInformation("Ratings backfill already complete — skipping.");
            return;
        }

        logger.LogInformation("Starting ratings backfill...");

        foreach (var leagueId in leaguesNeedingRecalc)
        {
            try
            {
                ratingsCalculator.Calculate(leagueId);
                logger.LogInformation("Calculated ELO ratings for league {LeagueId}.", leagueId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", leagueId);
            }
        }

        logger.LogInformation("Ratings backfill complete.");
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Re-rank continues with remaining replays/leagues even if one fails")]
    private async Task RerankAllGames(CancellationToken _)
    {
        using var activity = Telemetry.Source.StartActivity("Winner-Takes-All Re-rank");

        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("Database");
        await using var connection = new NpgsqlConnection(connectionString);

        // The data_fixes table ships in migration V0.10. The gateway may start against an
        // older schema during a rollout (code deployed before the migration is applied), so
        // skip gracefully rather than crashing — the re-rank runs on a later startup once the
        // schema is current.
        var tableExists = await connection.QuerySingleAsync<string?>(
            "SELECT to_regclass('public.data_fixes')::text");
        if (tableExists is null)
        {
            logger.LogInformation(
                "data_fixes table does not exist yet — skipping winner-takes-all re-rank until the schema is migrated.");
            return;
        }

        var markerCount = await connection.QuerySingleAsync<long>(
            "SELECT COUNT(*) FROM data_fixes WHERE name = 'winner-takes-all-rerank'");
        if (markerCount > 0)
        {
            logger.LogInformation("Winner-takes-all re-rank already applied — skipping.");
            return;
        }

        logger.LogInformation("Starting winner-takes-all re-rank...");

        var replayRepository = scope.ServiceProvider.GetRequiredService<IReplaysRepository>();
        var replayTextReader = scope.ServiceProvider.GetRequiredService<IReplayTextReader>();
        var leaguesRepository = scope.ServiceProvider.GetRequiredService<ILeaguesRepository>();
        var ratingsCalculator = scope.ServiceProvider.GetRequiredService<IRatingsCalculator>();

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
                    .Select(p => new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position, null, null, null))
                    .ToList();
                replayRepository.Update(replay with { Placements = placements });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to re-rank placements for replay {ReplayId}.", replay.Id);
            }
        }

        foreach (var leagueId in leaguesRepository.GetAll().Select(l => l.Id))
        {
            try
            {
                ratingsCalculator.Calculate(leagueId);
                logger.LogInformation("Recalculated ELO ratings for league {LeagueId}.", leagueId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to recalculate ELO ratings for league {LeagueId}.", leagueId);
            }
        }

        await connection.ExecuteAsync(
            "INSERT INTO data_fixes (name) VALUES ('winner-takes-all-rerank') ON CONFLICT (name) DO NOTHING");

        logger.LogInformation("Winner-takes-all re-rank complete.");
    }
}
