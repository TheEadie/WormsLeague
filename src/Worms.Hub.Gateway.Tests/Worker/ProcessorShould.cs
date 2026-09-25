using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Files;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Ratings;
using Worms.Hub.Gateway.Worker;
using Worms.Hub.Queues;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Fake;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.Tests.Worker;

[TestFixture]
internal sealed class ProcessorShould
{
    private const string ReplayFileName = "2024-03-01 20.15.30 [Online] alice, bob.WAgame";
    private const string LogFileName = "2024-03-01 20.15.30 [Online] alice, bob.log";
    private const string LeagueId = "redgate";

    private const string TwoTeamLogWithWinner = """
                                                Game Started at 2024-03-01 20:15:30 GMT
                                                Red: "alice" as "Alice Team"
                                                Blue: "bob" as "Bob Team"
                                                [00:00:00.00] ••• Alice Team (alice) starts turn
                                                [00:00:10.00] ••• Alice Team (alice) fires Bazooka
                                                [00:00:25.00] ••• Alice Team (alice) ends turn; time used: 20.00 sec turn, 5.00 sec retreat
                                                Alice Team wins the match!
                                                """;

    private const string TwoTeamLogWithoutWinner = """
                                                   Game Started at 2024-03-01 20:15:30 GMT
                                                   Red: "alice" as "Alice Team"
                                                   Blue: "bob" as "Bob Team"
                                                   """;

    private const string TwoTeamLogWithADraw = """
                                               Game Started at 2024-03-01 20:15:30 GMT
                                               Red: "alice" as "Alice Team"
                                               Blue: "bob" as "Bob Team"
                                               The round was drawn.
                                               """;

    private const string LogWithoutTeams = """
                                           Game Started at 2024-03-01 20:15:30 GMT
                                           Alice Team wins the match!
                                           """;

    private const string LogWithoutStartTime = """
                                               Red: "alice" as "Alice Team"
                                               Blue: "bob" as "Bob Team"
                                               Alice Team wins the match!
                                               """;

    private static readonly string TempReplayFolder =
        Path.Combine(Path.GetTempPath(), "worms-processor-tests-replays");

    private static readonly Replay SeededReplay = new(
        "7",
        "Friday night game",
        "Pending",
        ReplayFileName,
        null,
        LeagueId,
        null,
        null,
        null,
        null);

    private FakeHubStorage _storage = null!;
    private FakeReplaysToUpdateQueue _queue = null!;
    private MockFileSystem _fileSystem = null!;
    private FakeAnnouncer _announcer = null!;
    private FakeRatingsCalculator _ratingsCalculator = null!;
    private ServiceProvider _serviceProvider = null!;
    private Processor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = new FakeHubStorage();
        _queue = new FakeReplaysToUpdateQueue();
        _fileSystem = new MockFileSystem();
        _fileSystem.AddDirectory(TempReplayFolder);
        _announcer = new FakeAnnouncer();
        _ratingsCalculator = new FakeRatingsCalculator();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["Storage:TempReplayFolder"] = TempReplayFolder })
            .Build();

        _serviceProvider = new ServiceCollection()
            .AddWormsArmageddonFilesServices()
            .BuildServiceProvider();

        _processor = new Processor(
            _queue,
            _storage.Replays,
            _storage.Teams,
            new ReplayFiles(configuration, _fileSystem),
            _fileSystem,
            _announcer,
            _serviceProvider.GetRequiredService<IReplayTextReader>(),
            _ratingsCalculator,
            NullLogger<Processor>.Instance);
    }

    [TearDown]
    public void TearDown() => _serviceProvider.Dispose();

    [Test]
    public async Task MarkTheReplayAsProcessed()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        StoredReplay().Status.ShouldBe("Processed");
    }

    [Test]
    public async Task StoreTheFullLogOnTheReplay()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        StoredReplay().FullLog.ShouldBe(TwoTeamLogWithWinner);
    }

    [Test]
    public async Task StoreTheStartTimeAsTheReplayDate()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        StoredReplay().Date.ShouldBe(new DateTime(2024, 3, 1, 20, 15, 30));
    }

    [Test]
    public async Task StoreTheWinnerOnTheReplay()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        StoredReplay().Winner.ShouldBe("Alice Team");
    }

    [Test]
    public async Task StoreBothTeamsOnTheReplay()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        StoredReplay().Teams.ShouldBe(["Alice Team", "Bob Team"]);
    }

    [Test]
    public async Task StoreAPlacementForEachTeamWithTheWinnerInFirstPlace()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        StoredReplay().Placements.ShouldBe(
        [
            new ReplayPlacement("alice", "Alice Team", 1, null, null, null),
            new ReplayPlacement("bob", "Bob Team", 2, null, null, null)
        ]);
    }

    [Test]
    public async Task LeaveFieldsItDoesNotSetUnchanged()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        var stored = StoredReplay();
        stored.Id.ShouldBe(SeededReplay.Id);
        stored.Name.ShouldBe(SeededReplay.Name);
        stored.Filename.ShouldBe(SeededReplay.Filename);
        stored.LeagueId.ShouldBe(SeededReplay.LeagueId);
    }

    [Test]
    public async Task CreateATeamRecordForEachTeamInTheLog()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        _storage.Teams.GetAll()
            .Select(t => (t.Machine, t.TeamName))
            .ShouldBe([("alice", "Alice Team"), ("bob", "Bob Team")], ignoreOrder: true);
    }

    [Test]
    public async Task AnnounceTheWinnerAndPlacementsOnce()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        var announcement = _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem();
        announcement.Winner.ShouldBe("Alice Team");
        announcement.Placements.ShouldBe([new PlacementInfo("Alice Team", 1), new PlacementInfo("Bob Team", 2)]);
    }

    [Test]
    public async Task DeleteTheMessageFromTheQueue()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        _queue.Deleted.ShouldBe([_queue.PendingMessageDetails!]);
    }

    [Test]
    public async Task AnnounceEachPlayersEloAndRankChangeForALeagueReplay()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ratingsCalculator.Result = new LeagueRatingsChange(
            [
                new PlayerStanding("auth|alice", "Alice", 2, 1000),
                new PlayerStanding("auth|bob", "Bob", 1, 1010)
            ],
            [
                new PlayerStanding("auth|alice", "Alice", 1, 1016),
                new PlayerStanding("auth|bob", "Bob", 2, 994)
            ]);

        await _processor.UpdateReplay();

        _ratingsCalculator.CalculatedLeagues.ShouldBe([LeagueId]);
        var announcement = _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem();
        announcement.Leaderboard.ShouldBe(
        [
            new LeaderboardEntry(1, 1016, "Alice", 16, -1),
            new LeaderboardEntry(2, 994, "Bob", -16, 1)
        ]);
        announcement.LeaderboardFailureNote.ShouldBeNull();
    }

    [Test]
    public async Task LeaveChangesBlankForAPlayerWhoseRatingAndRankAreUnchanged()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ratingsCalculator.Result = new LeagueRatingsChange(
            [
                new PlayerStanding("auth|alice", "Alice", 2, 1000),
                new PlayerStanding("auth|bob", "Bob", 1, 1010),
                new PlayerStanding("auth|carol", "Carol", 3, 990)
            ],
            [
                new PlayerStanding("auth|alice", "Alice", 1, 1016),
                new PlayerStanding("auth|bob", "Bob", 2, 994),
                new PlayerStanding("auth|carol", "Carol", 3, 990)
            ]);

        await _processor.UpdateReplay();

        var announcement = _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem();
        var carol = announcement.Leaderboard.ShouldNotBeNull().Single(e => e.DisplayName == "Carol");
        carol.EloDelta.ShouldBeNull();
        carol.PositionChange.ShouldBeNull();
    }

    [Test]
    public async Task SendNoLeaderboardWhenTheCalculatorReturnsNoPlayers()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ratingsCalculator.Result = new LeagueRatingsChange([], []);

        await _processor.UpdateReplay();

        var announcement = _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem();
        announcement.Leaderboard.ShouldBeNull();
        announcement.LeaderboardFailureNote.ShouldBeNull();
    }

    [Test]
    public async Task SendAFailureNoteInsteadOfALeaderboardWhenTheCalculatorThrows()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ratingsCalculator.ExceptionToThrow = new InvalidOperationException("Ratings failed");

        await _processor.UpdateReplay();

        var announcement = _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem();
        announcement.Leaderboard.ShouldBeNull();
        announcement.LeaderboardFailureNote.ShouldBe("ELO leaderboard unavailable.");
    }

    [Test]
    public async Task StillUpdateTheReplayAndDeleteTheMessageWhenTheCalculatorThrows()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ratingsCalculator.ExceptionToThrow = new InvalidOperationException("Ratings failed");

        await _processor.UpdateReplay();

        StoredReplay().Status.ShouldBe("Processed");
        _queue.Deleted.ShouldBe([_queue.PendingMessageDetails!]);
    }

    [Test]
    public async Task NotCalculateRatingsForAReplayOutsideALeague()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner, leagueId: null);

        await _processor.UpdateReplay();

        _ratingsCalculator.CalculatedLeagues.ShouldBeEmpty();
        var announcement = _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem();
        announcement.Leaderboard.ShouldBeNull();
        announcement.LeaderboardFailureNote.ShouldBeNull();
    }

    [Test]
    public async Task StoreNoWinnerOrPositionsWhenTheLogHasNoWinner()
    {
        await GivenAQueuedReplay(TwoTeamLogWithoutWinner);

        await _processor.UpdateReplay();

        var stored = StoredReplay();
        stored.Winner.ShouldBeNull();
        stored.Placements.ShouldBe(
        [
            new ReplayPlacement("alice", "Alice Team", null, null, null, null),
            new ReplayPlacement("bob", "Bob Team", null, null, null, null)
        ]);
    }

    [Test]
    public async Task StoreAndAnnounceEveryTeamInFirstPlaceWhenTheLogIsADraw()
    {
        await GivenAQueuedReplay(TwoTeamLogWithADraw);

        await _processor.UpdateReplay();

        var stored = StoredReplay();
        stored.Winner.ShouldBe("Draw");
        stored.Placements.ShouldBe(
        [
            new ReplayPlacement("alice", "Alice Team", 1, null, null, null),
            new ReplayPlacement("bob", "Bob Team", 1, null, null, null)
        ]);
        _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem().Placements.ShouldBe(
            [new PlacementInfo("Alice Team", 1), new PlacementInfo("Bob Team", 1)]);
    }

    [Test]
    public async Task StillCreateTeamRecordsWhenTheLogHasNoWinner()
    {
        await GivenAQueuedReplay(TwoTeamLogWithoutWinner);

        await _processor.UpdateReplay();

        _storage.Teams.GetAll()
            .Select(t => (t.Machine, t.TeamName))
            .ShouldBe([("alice", "Alice Team"), ("bob", "Bob Team")], ignoreOrder: true);
    }

    [Test]
    public async Task AnnounceNoPlacementsWhenTheLogHasNoWinner()
    {
        await GivenAQueuedReplay(TwoTeamLogWithoutWinner);

        await _processor.UpdateReplay();

        _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem().Placements.ShouldBeNull();
    }

    [Test]
    public async Task StoreNoTeamsOrPlacementsWhenTheLogHasNoTeams()
    {
        await GivenAQueuedReplay(LogWithoutTeams);

        await _processor.UpdateReplay();

        var stored = StoredReplay();
        stored.Teams.ShouldBeNull();
        stored.Placements.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Test]
    public async Task CreateNoTeamRecordsWhenTheLogHasNoTeams()
    {
        await GivenAQueuedReplay(LogWithoutTeams);

        await _processor.UpdateReplay();

        _storage.Teams.GetAll().ShouldBeEmpty();
    }

    [Test]
    public async Task AnnounceNoPlacementsWhenTheLogHasNoTeams()
    {
        await GivenAQueuedReplay(LogWithoutTeams);

        await _processor.UpdateReplay();

        _announcer.GameCompleteAnnouncements.ShouldHaveSingleItem().Placements.ShouldBeNull();
    }

    [Test]
    public async Task StoreNoDateWhenTheLogHasNoStartTime()
    {
        await GivenAQueuedReplay(LogWithoutStartTime);

        await _processor.UpdateReplay();

        StoredReplay().Date.ShouldBeNull();
    }

    [Test]
    public async Task DoNothingWhenTheQueueIsEmpty()
    {
        _storage.Replays.Seed(SeededReplay);
        AddReplayFile();
        AddLogFile(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        ShouldNotHaveProcessedTheReplay();
    }

    [Test]
    public async Task DoNothingWhenTheReplayFileIsMissing()
    {
        _storage.Replays.Seed(SeededReplay);
        AddLogFile(TwoTeamLogWithWinner);
        await _queue.EnqueueMessage(new ReplayToUpdateMessage(ReplayFileName));

        await _processor.UpdateReplay();

        ShouldNotHaveProcessedTheReplay();
    }

    [Test]
    public async Task DoNothingWhenTheLogFileIsMissing()
    {
        _storage.Replays.Seed(SeededReplay);
        AddReplayFile();
        await _queue.EnqueueMessage(new ReplayToUpdateMessage(ReplayFileName));

        await _processor.UpdateReplay();

        ShouldNotHaveProcessedTheReplay();
    }

    [Test]
    public async Task DoNothingWhenNoReplayRecordMatchesTheMessage()
    {
        _storage.Replays.Seed(SeededReplay with { Filename = "some other replay.WAgame" });
        AddReplayFile();
        AddLogFile(TwoTeamLogWithWinner);
        await _queue.EnqueueMessage(new ReplayToUpdateMessage(ReplayFileName));

        await _processor.UpdateReplay();

        _storage.Replays.GetAll().ShouldHaveSingleItem().Status.ShouldBe("Pending");
        _storage.Teams.GetAll().ShouldBeEmpty();
        _announcer.GameCompleteAnnouncements.ShouldBeEmpty();
        _announcer.GameStartingAnnouncements.ShouldBeEmpty();
        _queue.Deleted.ShouldBeEmpty();
    }

    private async Task GivenAQueuedReplay(string log, string? leagueId = LeagueId)
    {
        _storage.Replays.Seed(SeededReplay with { LeagueId = leagueId });
        AddReplayFile();
        AddLogFile(log);
        await _queue.EnqueueMessage(new ReplayToUpdateMessage(ReplayFileName));
    }

    private void AddReplayFile() =>
        _fileSystem.AddFile(Path.Combine(TempReplayFolder, ReplayFileName), new MockFileData(string.Empty));

    private void AddLogFile(string log) =>
        _fileSystem.AddFile(Path.Combine(TempReplayFolder, LogFileName), new MockFileData(log));

    private Replay StoredReplay() => _storage.Replays.GetAll().Single(r => r.Id == SeededReplay.Id);

    private void ShouldNotHaveProcessedTheReplay()
    {
        StoredReplay().ShouldBe(SeededReplay);
        _storage.Teams.GetAll().ShouldBeEmpty();
        _announcer.GameCompleteAnnouncements.ShouldBeEmpty();
        _announcer.GameStartingAnnouncements.ShouldBeEmpty();
        _queue.Deleted.ShouldBeEmpty();
    }
}
