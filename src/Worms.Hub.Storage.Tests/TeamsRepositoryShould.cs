using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Tests;

[TestFixture]
[Category("Integration")]
internal sealed class TeamsRepositoryShould
{
    private const int NonExistentId = -1;

    private ServiceProvider _services = null!;
    private ITeamsRepository _repository = null!;
    private IPlayersRepository _players = null!;

    [OneTimeSetUp]
    public void CreateRepository()
    {
        _services = ComposeDatabase.CreateHubStorageServices();
        _repository = _services.GetRequiredService<ITeamsRepository>();
        _players = _services.GetRequiredService<IPlayersRepository>();
    }

    [OneTimeTearDown]
    public void DisposeServices() => _services.Dispose();

    [Test]
    public void ReturnAnUnclaimedTeamAfterUpsertingIt()
    {
        var machine = UniqueMachine();

        _repository.Upsert(machine, "Contract Test Team");

        var team = _repository.GetAll().Single(t => t.Machine == machine);
        team.ShouldBe(new Team(team.Id, machine, "Contract Test Team", null, null));
        _repository.GetById(team.Id).ShouldBe(team);
    }

    [Test]
    public void ReturnNoTeamForANonExistentId()
    {
        var team = Should.NotThrow(() => _repository.GetById(NonExistentId));

        team.ShouldBeNull();
    }

    [Test]
    public void KeepASingleTeamWhenUpsertingADuplicate()
    {
        var original = CreateTeam();

        Should.NotThrow(() => _repository.Upsert(original.Machine, "Contract Test Team"));

        var teams = _repository.GetAll()
            .Where(t => t.Machine == original.Machine && t.TeamName == "Contract Test Team");
        teams.ShouldHaveSingleItem().Id.ShouldBe(original.Id);
    }

    [Test]
    public void CreateSeparateTeamsForDifferentTeamNamesOnTheSameMachine()
    {
        var machine = UniqueMachine();

        _repository.Upsert(machine, "First Team");
        _repository.Upsert(machine, "Second Team");

        var teams = _repository.GetAll().Where(t => t.Machine == machine).ToList();
        var first = teams.Where(t => t.TeamName == "First Team").ShouldHaveSingleItem();
        var second = teams.Where(t => t.TeamName == "Second Team").ShouldHaveSingleItem();
        first.Id.ShouldNotBe(second.Id);
    }

    [Test]
    public void ReturnTheClaimingPlayerAfterClaimingATeam()
    {
        var team = CreateTeam();
        var player = _players.Create(new Player(UniqueAuthSubject(), "Contract Test Player"));

        _repository.SetPlayerClaim(team.Id, player.AuthSubject);

        _repository.GetById(team.Id).ShouldBe(team with
        {
            ClaimedByPlayerName = "Contract Test Player",
            ClaimedByAuthSubject = player.AuthSubject
        });
    }

    [Test]
    public void ReturnAnUnclaimedTeamAfterRemovingTheClaim()
    {
        var team = CreateTeam();
        var player = _players.Create(new Player(UniqueAuthSubject(), "Contract Test Player"));
        _repository.SetPlayerClaim(team.Id, player.AuthSubject);

        _repository.SetPlayerClaim(team.Id, null);

        _repository.GetById(team.Id).ShouldBe(team with { ClaimedByPlayerName = null, ClaimedByAuthSubject = null });
    }

    [Test]
    public void RejectAClaimByAnUnknownAuthSubjectAndKeepTheTeamUnclaimed()
    {
        var team = CreateTeam();

        var exception = Should.Throw<PostgresException>(
            () => _repository.SetPlayerClaim(team.Id, UniqueAuthSubject()));

        exception.SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
        _repository.GetById(team.Id).ShouldBe(team);
    }

    [Test]
    public void IgnoreAClaimOnANonExistentTeam()
    {
        var player = _players.Create(new Player(UniqueAuthSubject(), "Contract Test Player"));

        Should.NotThrow(() => _repository.SetPlayerClaim(NonExistentId, player.AuthSubject));

        _repository.GetById(NonExistentId).ShouldBeNull();
    }

    private Team CreateTeam()
    {
        var machine = UniqueMachine();
        _repository.Upsert(machine, "Contract Test Team");
        return _repository.GetAll().Single(t => t.Machine == machine);
    }

    private static string UniqueMachine() => $"contract-test-{Guid.NewGuid():N}";

    private static string UniqueAuthSubject() => $"contract-test|{Guid.NewGuid():N}";
}
