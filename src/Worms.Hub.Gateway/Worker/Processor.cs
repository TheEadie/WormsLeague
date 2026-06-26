using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Ratings;
using Worms.Hub.Queues;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.Worker;

internal sealed class Processor(
    IMessageQueue<ReplayToUpdateMessage> messageQueue,
    IReplaysRepository replayRepository,
    ITeamsRepository teamsRepository,
    ReplayFiles replayFiles,
    IAnnouncer announcer,
    IReplayTextReader replayTextReader,
    IRatingsCalculator ratingsCalculator,
    ILogger<Processor> logger)
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "ELO calculation failure must not block replay processing")]
    public async Task UpdateReplay()
    {
        logger.LogInformation("Starting replay updater...");

        var (message, token, activityContext) = await messageQueue.DequeueMessage();

        using var span = Telemetry.Source.StartActivity(
            "Update Replay",
            ActivityKind.Consumer,
            activityContext);

        if (message is null || token is null)
        {
            logger.LogInformation("No messages to process.");
            return;
        }

        // Check replay is in the folder
        var replayPath = replayFiles.GetReplayPath(message.ReplayFileName);
        if (!File.Exists(replayPath))
        {
            logger.LogError("Replay not found on disk: {ReplayPath}", replayPath);
            return;
        }

        // Check replay has a log file generated
        var logPath = replayFiles.GetLogPath(message.ReplayFileName);
        if (!File.Exists(logPath))
        {
            logger.LogError("Log file not found: {LogPath}", logPath);
            return;
        }

        // Check the replay exists in the database
        var replay = replayRepository.GetAll().FirstOrDefault(r => r.Filename == message.ReplayFileName);
        if (replay is null)
        {
            logger.LogError("Replay not found in database: {ReplayFileName}", message.ReplayFileName);
            return;
        }

        var replayLog = await File.ReadAllTextAsync(logPath);

        // Parse the replay log
        var replayModel = replayTextReader.GetModel(replayLog);

        // Update the database with the log and parsed fields
        var updatedReplay = replay with
        {
            Status = "Processed",
            FullLog = replayLog,
            Date = replayModel.Date == default ? null : replayModel.Date,
            Winner = string.IsNullOrEmpty(replayModel.Winner) ? null : replayModel.Winner,
            Teams = replayModel.Placements.Count > 0
                ? replayModel.Placements.Select(p => p.Team.Name).ToList()
                : null,
            Placements = replayModel.Placements
                .Select(p => new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position, null, null, null))
                .ToList()
        };
        replayRepository.Update(updatedReplay);

        // Upsert teams from placements
        foreach (var placement in replayModel.Placements)
        {
            teamsRepository.Upsert(placement.Team.Machine, placement.Team.Name);
        }

        // Calculate ELO ratings for the league
        List<LeaderboardEntry>? leaderboard = null;
        string? leaderboardFailureNote = null;

        if (updatedReplay.LeagueId is not null)
        {
            try
            {
                var change = ratingsCalculator.Calculate(updatedReplay.LeagueId);
                leaderboard = BuildLeaderboard(change);
                if (leaderboard.Count == 0)
                {
                    leaderboard = null;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", updatedReplay.LeagueId);
                leaderboardFailureNote = "ELO leaderboard unavailable.";
            }
        }

        // Announce game complete
        IReadOnlyList<PlacementInfo>? placements = null;
        if (replayModel.Placements.Any(p => p.Position.HasValue))
        {
            placements = replayModel.Placements
                .Where(p => p.Position.HasValue)
                .Select(p => new PlacementInfo(p.Team.Name, p.Position!.Value))
                .ToList();
        }

        await announcer.AnnounceGameComplete(replayModel.Winner, placements, leaderboard, leaderboardFailureNote);

        // Delete the message from the queue
        await messageQueue.DeleteMessage(token);

        logger.LogInformation("Replay updater finished.");
    }

    private static List<LeaderboardEntry> BuildLeaderboard(LeagueRatingsChange change)
    {
        var before = change.Before.ToDictionary(p => p.PlayerAuthSubject);

        return change.After.Select(post =>
        {
            int? eloDelta = null;
            int? positionChange = null;
            if (before.TryGetValue(post.PlayerAuthSubject, out var pre))
            {
                var elo = post.Rating - pre.Rating;
                if (elo != 0)
                {
                    eloDelta = elo;
                }
                var rank = post.Rank - pre.Rank;
                if (rank != 0)
                {
                    positionChange = rank;
                }
            }

            return new LeaderboardEntry(post.Rank, post.Rating, post.DisplayName, eloDelta, positionChange);
        }).ToList();
    }
}
