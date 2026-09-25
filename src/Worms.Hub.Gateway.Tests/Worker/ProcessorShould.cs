using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Files;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Ratings;
using Worms.Hub.Gateway.Worker;
using Worms.Hub.Queues;
using Worms.Hub.Queues.Fake;
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

    private static readonly ReplayToUpdateMessage QueuedMessage = new(ReplayFileName);

    private FakeHubStorage _storage = null!;
    private FakeMessageQueue<ReplayToUpdateMessage> _queue = null!;
    private MockFileSystem _fileSystem = null!;
    private IAnnouncer _announcer = null!;
    private IRatingsCalculator _ratingsCalculator = null!;
    private ServiceProvider _serviceProvider = null!;
    private Processor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = new FakeHubStorage();
        _queue = new FakeMessageQueue<ReplayToUpdateMessage>();
        _fileSystem = new MockFileSystem();
        _fileSystem.AddDirectory(TempReplayFolder);
        _announcer = Substitute.For<IAnnouncer>();
        _ratingsCalculator = Substitute.For<IRatingsCalculator>();
        _ = _ratingsCalculator.Calculate(Arg.Any<string>()).Returns(new LeagueRatingsChange([], []));

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

        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            "Alice Team",
            SequenceOf(new PlacementInfo("Alice Team", 1), new PlacementInfo("Bob Team", 2)),
            Arg.Any<IReadOnlyList<LeaderboardEntry>?>(),
            Arg.Any<string?>());
    }

    [Test]
    public async Task DeleteTheMessageFromTheQueue()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);

        await _processor.UpdateReplay();

        _queue.Deleted.ShouldBe([QueuedMessage]);
        _queue.Pending.ShouldBeEmpty();
    }

    [Test]
    public async Task AnnounceEachPlayersEloAndRankChangeForALeagueReplay()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ = _ratingsCalculator.Calculate(LeagueId).Returns(new LeagueRatingsChange(
            [
                new PlayerStanding("auth|alice", "Alice", 2, 1000),
                new PlayerStanding("auth|bob", "Bob", 1, 1010)
            ],
            [
                new PlayerStanding("auth|alice", "Alice", 1, 1016),
                new PlayerStanding("auth|bob", "Bob", 2, 994)
            ]));

        await _processor.UpdateReplay();

        _ = _ratingsCalculator.Received(1).Calculate(Arg.Any<string>());
        _ = _ratingsCalculator.Received(1).Calculate(LeagueId);
        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<PlacementInfo>?>(),
            SequenceOf(
                new LeaderboardEntry(1, 1016, "Alice", 16, -1),
                new LeaderboardEntry(2, 994, "Bob", -16, 1)),
            Arg.Is<string?>(value: null));
    }

    [Test]
    public async Task LeaveChangesBlankForAPlayerWhoseRatingAndRankAreUnchanged()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ = _ratingsCalculator.Calculate(LeagueId).Returns(new LeagueRatingsChange(
            [
                new PlayerStanding("auth|alice", "Alice", 2, 1000),
                new PlayerStanding("auth|bob", "Bob", 1, 1010),
                new PlayerStanding("auth|carol", "Carol", 3, 990)
            ],
            [
                new PlayerStanding("auth|alice", "Alice", 1, 1016),
                new PlayerStanding("auth|bob", "Bob", 2, 994),
                new PlayerStanding("auth|carol", "Carol", 3, 990)
            ]));

        await _processor.UpdateReplay();

        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<PlacementInfo>?>(),
            Arg.Is<IReadOnlyList<LeaderboardEntry>?>(leaderboard =>
                leaderboard != null
                && leaderboard.Any(e => e.DisplayName == "Carol" && e.EloDelta == null && e.PositionChange == null)),
            Arg.Any<string?>());
    }

    [Test]
    public async Task SendNoLeaderboardWhenTheCalculatorReturnsNoPlayers()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ = _ratingsCalculator.Calculate(LeagueId).Returns(new LeagueRatingsChange([], []));

        await _processor.UpdateReplay();

        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<PlacementInfo>?>(),
            Arg.Is<IReadOnlyList<LeaderboardEntry>?>(value: null),
            Arg.Is<string?>(value: null));
    }

    [Test]
    public async Task SendAFailureNoteInsteadOfALeaderboardWhenTheCalculatorThrows()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ = _ratingsCalculator.Calculate(LeagueId).Throws(new InvalidOperationException("Ratings failed"));

        await _processor.UpdateReplay();

        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<PlacementInfo>?>(),
            Arg.Is<IReadOnlyList<LeaderboardEntry>?>(value: null),
            "ELO leaderboard unavailable.");
    }

    [Test]
    public async Task StillUpdateTheReplayAndDeleteTheMessageWhenTheCalculatorThrows()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner);
        _ = _ratingsCalculator.Calculate(LeagueId).Throws(new InvalidOperationException("Ratings failed"));

        await _processor.UpdateReplay();

        StoredReplay().Status.ShouldBe("Processed");
        _queue.Deleted.ShouldBe([QueuedMessage]);
        _queue.Pending.ShouldBeEmpty();
    }

    [Test]
    public async Task NotCalculateRatingsForAReplayOutsideALeague()
    {
        await GivenAQueuedReplay(TwoTeamLogWithWinner, leagueId: null);

        await _processor.UpdateReplay();

        _ = _ratingsCalculator.DidNotReceive().Calculate(Arg.Any<string>());
        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<PlacementInfo>?>(),
            Arg.Is<IReadOnlyList<LeaderboardEntry>?>(value: null),
            Arg.Is<string?>(value: null));
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
        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            SequenceOf(new PlacementInfo("Alice Team", 1), new PlacementInfo("Bob Team", 1)),
            Arg.Any<IReadOnlyList<LeaderboardEntry>?>(),
            Arg.Any<string?>());
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

        await ShouldHaveAnnouncedNoPlacementsOnce();
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

        await ShouldHaveAnnouncedNoPlacementsOnce();
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

        await ShouldNotHaveProcessedTheReplay();
    }

    [Test]
    public async Task DoNothingWhenTheReplayFileIsMissing()
    {
        _storage.Replays.Seed(SeededReplay);
        AddLogFile(TwoTeamLogWithWinner);
        await _queue.EnqueueMessage(QueuedMessage);

        await _processor.UpdateReplay();

        await ShouldNotHaveProcessedTheReplay();
    }

    [Test]
    public async Task DoNothingWhenTheLogFileIsMissing()
    {
        _storage.Replays.Seed(SeededReplay);
        AddReplayFile();
        await _queue.EnqueueMessage(QueuedMessage);

        await _processor.UpdateReplay();

        await ShouldNotHaveProcessedTheReplay();
    }

    [Test]
    public async Task DoNothingWhenNoReplayRecordMatchesTheMessage()
    {
        _storage.Replays.Seed(SeededReplay with { Filename = "some other replay.WAgame" });
        AddReplayFile();
        AddLogFile(TwoTeamLogWithWinner);
        await _queue.EnqueueMessage(QueuedMessage);

        await _processor.UpdateReplay();

        _storage.Replays.GetAll().ShouldHaveSingleItem().Status.ShouldBe("Pending");
        _storage.Teams.GetAll().ShouldBeEmpty();
        await ShouldNotHaveAnnouncedAnything();
        _queue.Deleted.ShouldBeEmpty();
    }

    private async Task GivenAQueuedReplay(string log, string? leagueId = LeagueId)
    {
        _storage.Replays.Seed(SeededReplay with { LeagueId = leagueId });
        AddReplayFile();
        AddLogFile(log);
        await _queue.EnqueueMessage(QueuedMessage);
    }

    private static IReadOnlyList<T>? SequenceOf<T>(params T[] expected) =>
        Arg.Is<IReadOnlyList<T>?>(actual => actual != null && actual.SequenceEqual(expected));

    private void AddReplayFile() =>
        _fileSystem.AddFile(Path.Combine(TempReplayFolder, ReplayFileName), new MockFileData(string.Empty));

    private void AddLogFile(string log) =>
        _fileSystem.AddFile(Path.Combine(TempReplayFolder, LogFileName), new MockFileData(log));

    private Replay StoredReplay() => _storage.Replays.GetAll().Single(r => r.Id == SeededReplay.Id);

    private Task ShouldHaveAnnouncedGameCompleteOnce() =>
        _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<PlacementInfo>?>(),
            Arg.Any<IReadOnlyList<LeaderboardEntry>?>(),
            Arg.Any<string?>());

    private async Task ShouldHaveAnnouncedNoPlacementsOnce()
    {
        await ShouldHaveAnnouncedGameCompleteOnce();
        await _announcer.Received(1).AnnounceGameComplete(
            Arg.Any<string>(),
            Arg.Is<IReadOnlyList<PlacementInfo>?>(value: null),
            Arg.Any<IReadOnlyList<LeaderboardEntry>?>(),
            Arg.Any<string?>());
    }

    private async Task ShouldNotHaveAnnouncedAnything()
    {
        await _announcer.DidNotReceiveWithAnyArgs().AnnounceGameComplete(null!);
        await _announcer.DidNotReceiveWithAnyArgs().AnnounceGameStarting(null!);
    }

    private async Task ShouldNotHaveProcessedTheReplay()
    {
        StoredReplay().ShouldBe(SeededReplay);
        _storage.Teams.GetAll().ShouldBeEmpty();
        await ShouldNotHaveAnnouncedAnything();
        _queue.Deleted.ShouldBeEmpty();
    }
}
