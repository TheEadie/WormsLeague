using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Tests;

[TestFixture]
[Category("Integration")]
internal sealed class LeaguesRepositoryShould
{
    private const string SeededLeagueId = "redgate";
    private const string SeededLeagueName = "Redgate";
    private const string OtherSeededLeagueId = "speed-worms";
    private const string TeamName = "Contract Test Team";

    private ServiceProvider _services = null!;
    private ILeaguesRepository _repository = null!;
    private IReplaysRepository _replays = null!;

    [OneTimeSetUp]
    public void CreateRepository()
    {
        _services = ComposeDatabase.CreateHubStorageServices();
        _repository = _services.GetRequiredService<ILeaguesRepository>();
        _replays = _services.GetRequiredService<IReplaysRepository>();
    }

    [OneTimeTearDown]
    public void DisposeServices() => _services.Dispose();

    [Test]
    public void IncludeTheSeededLeagueInGetAll() =>
        _repository.GetAll().ShouldContain(new LeagueDb(SeededLeagueId, SeededLeagueName));

    [Test]
    public void ReturnTheSeededLeagueById() =>
        _repository.GetById(SeededLeagueId).ShouldBe(new LeagueDb(SeededLeagueId, SeededLeagueName));

    [Test]
    public void ReturnNoLeagueForANonExistentId()
    {
        var league = Should.NotThrow(() => _repository.GetById($"contract-test-{Guid.NewGuid():N}"));

        league.ShouldBeNull();
    }

    [Test]
    public void ReturnTheLeagueOfAProcessedReplayTheTeamPlayedIn()
    {
        var machine = UniqueMachine();
        CreateReplay(machine, TeamName, "Processed", SeededLeagueId);

        _repository.GetLeaguesTeamPlaysIn(machine, TeamName).ShouldBe([SeededLeagueId]);
    }

    [Test]
    public void ReturnEachLeagueOnceWhenTheTeamPlayedInItMoreThanOnce()
    {
        var machine = UniqueMachine();
        CreateReplay(machine, TeamName, "Processed", SeededLeagueId);
        CreateReplay(machine, TeamName, "Processed", SeededLeagueId);

        _repository.GetLeaguesTeamPlaysIn(machine, TeamName).ShouldBe([SeededLeagueId]);
    }

    [Test]
    public void ReturnEveryLeagueTheTeamPlayedIn()
    {
        var machine = UniqueMachine();
        CreateReplay(machine, TeamName, "Processed", SeededLeagueId);
        CreateReplay(machine, TeamName, "Processed", OtherSeededLeagueId);

        _repository.GetLeaguesTeamPlaysIn(machine, TeamName)
            .ShouldBe([SeededLeagueId, OtherSeededLeagueId], ignoreOrder: true);
    }

    [Test]
    public void IgnoreReplaysThatAreNotProcessed()
    {
        var machine = UniqueMachine();
        CreateReplay(machine, TeamName, "Pending", SeededLeagueId);

        _repository.GetLeaguesTeamPlaysIn(machine, TeamName).ShouldBeEmpty();
    }

    [Test]
    public void IgnoreReplaysWithoutALeague()
    {
        var machine = UniqueMachine();
        CreateReplay(machine, TeamName, "Processed", null);

        _repository.GetLeaguesTeamPlaysIn(machine, TeamName).ShouldBeEmpty();
    }

    [Test]
    public void ReturnNoLeaguesForAnUnknownTeam()
    {
        var leagues = Should.NotThrow(() => _repository.GetLeaguesTeamPlaysIn(UniqueMachine(), TeamName));

        leagues.ShouldBeEmpty();
    }

    [Test]
    public void ReturnNoLeaguesForAnotherTeamOnTheSameMachine()
    {
        var machine = UniqueMachine();
        CreateReplay(machine, "First Team", "Processed", SeededLeagueId);

        _repository.GetLeaguesTeamPlaysIn(machine, "Second Team").ShouldBeEmpty();
    }

    [Test]
    public void ReturnNoLeaguesForTheSameTeamNameOnAnotherMachine()
    {
        CreateReplay(UniqueMachine(), TeamName, "Processed", SeededLeagueId);

        _repository.GetLeaguesTeamPlaysIn(UniqueMachine(), TeamName).ShouldBeEmpty();
    }

    private void CreateReplay(string machine, string teamName, string status, string? leagueId) =>
        _ = _replays.Create(new Replay(
            "0",
            "Contract Test Replay",
            status,
            "contract-test.WAgame",
            null,
            leagueId,
            null,
            null,
            [teamName],
            [new ReplayPlacement(machine, teamName, 1, null, null, null)]));

    private static string UniqueMachine() => $"contract-test-{Guid.NewGuid():N}";
}
