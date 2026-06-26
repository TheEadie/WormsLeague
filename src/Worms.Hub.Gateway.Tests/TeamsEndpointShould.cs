using System.Diagnostics.CodeAnalysis;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class TeamsEndpointShould
{
    private const string TeamsUrl = "api/v1/teams";

    private GatewayTestHost _host = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _host = new GatewayTestHost();
        _client = _host.CreateClient(TestJwt.WithAccessRole());
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _host.Dispose();
    }

    // ── GET /api/v1/teams ────────────────────────────────────────────────────

    [Test]
    public async Task ReturnEmptyListWhenNoTeamsExist()
    {
        _host.TeamsRepository.GetAll().Returns([]);

        var response = await _client.GetAsync(new Uri(TeamsUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var teams = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TeamDto>>();
        teams.ShouldNotBeNull();
        teams.ShouldBeEmpty();
    }

    [Test]
    public async Task ReturnAllTeamsMappingFields()
    {
        _host.TeamsRepository.GetAll().Returns(
        [
            new Team(1, "Machine1", "TeamA", null, null),
            new Team(2, "Machine2", "TeamB", "Alice", "test-user"),
            new Team(3, "Machine3", "TeamC", "Bob", "other-user")
        ]);

        var response = await _client.GetAsync(new Uri(TeamsUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var teams = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TeamDto>>();
        teams.ShouldNotBeNull();
        teams.Count.ShouldBe(3);

        var unclaimed = teams.FirstOrDefault(t => t.Id == 1);
        unclaimed.ShouldNotBeNull();
        unclaimed.Machine.ShouldBe("Machine1");
        unclaimed.TeamName.ShouldBe("TeamA");
        unclaimed.ClaimedBy.ShouldBeNull();

        var mine = teams.FirstOrDefault(t => t.Id == 2);
        mine.ShouldNotBeNull();
        mine.Machine.ShouldBe("Machine2");
        mine.TeamName.ShouldBe("TeamB");
        mine.ClaimedBy.ShouldBe("Alice");

        var another = teams.FirstOrDefault(t => t.Id == 3);
        another.ShouldNotBeNull();
        another.Machine.ShouldBe("Machine3");
        another.TeamName.ShouldBe("TeamC");
        another.ClaimedBy.ShouldBe("Bob");
    }

    [Test]
    public async Task SetIsMyTeamTrueOnlyForTeamsClaimedByCaller()
    {
        _host.TeamsRepository.GetAll().Returns(
        [
            new Team(1, "M1", "T1", null, null),
            new Team(2, "M2", "T2", "Alice", "test-user"),
            new Team(3, "M3", "T3", "Bob", "other-user")
        ]);

        var response = await _client.GetAsync(new Uri(TeamsUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var teams = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TeamDto>>();
        teams.ShouldNotBeNull();

        teams.First(t => t.Id == 1).IsMyTeam.ShouldBeFalse();
        teams.First(t => t.Id == 2).IsMyTeam.ShouldBeTrue();
        teams.First(t => t.Id == 3).IsMyTeam.ShouldBeFalse();
    }

    // ── PUT /api/v1/teams ────────────────────────────────────────────────────

    [Test]
    public async Task Return404WhenTeamDoesNotExist()
    {
        _host.TeamsRepository.GetById(999).Returns((Team?)null);

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(999, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        _host.PlayersRepository.DidNotReceive().Create(Arg.Any<Player>());
        _host.TeamsRepository.DidNotReceive().SetPlayerClaim(Arg.Any<int>(), Arg.Any<string?>());
        _host.RatingsCalculator.DidNotReceive().CalculateForTeam(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task CreatePlayerAndClaimWhenUnclaimedAndNoPlayerExists_BodyDisplayName()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject("test-user").Returns((Player?)null);
        _host.PlayersRepository.Create(Arg.Any<Player>()).Returns(ci => ci.Arg<Player>());

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, "Body Name"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.PlayersRepository.Received()
            .Create(Arg.Is<Player>(p => p.AuthSubject == "test-user" && p.DisplayName == "Body Name"));
        _host.TeamsRepository.Received().SetPlayerClaim(1, "test-user");
    }

    [Test]
    public async Task FallsBackToNicknameClaim()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject("test-user").Returns((Player?)null);
        _host.PlayersRepository.Create(Arg.Any<Player>()).Returns(ci => ci.Arg<Player>());

        using var client = _host.CreateClient(TestJwt.WithAccessRole(nickname: "NickFromToken"));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.PlayersRepository.Received()
            .Create(Arg.Is<Player>(p => p.AuthSubject == "test-user" && p.DisplayName == "NickFromToken"));
    }

    [Test]
    public async Task FallsBackToNameClaimWhenNoNickname()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject("test-user").Returns((Player?)null);
        _host.PlayersRepository.Create(Arg.Any<Player>()).Returns(ci => ci.Arg<Player>());

        using var client = _host.CreateClient(TestJwt.WithAccessRole(name: "NameFromToken"));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.PlayersRepository.Received()
            .Create(Arg.Is<Player>(p => p.AuthSubject == "test-user" && p.DisplayName == "NameFromToken"));
    }

    [Test]
    public async Task FallsBackToSubjectWhenNoNicknameOrName()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject("subject-xyz").Returns((Player?)null);
        _host.PlayersRepository.Create(Arg.Any<Player>()).Returns(ci => ci.Arg<Player>());

        using var client = _host.CreateClient(TestJwt.WithAccessRole(subject: "subject-xyz"));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.PlayersRepository.Received()
            .Create(Arg.Is<Player>(p => p.AuthSubject == "subject-xyz" && p.DisplayName == "subject-xyz"));
        _host.TeamsRepository.Received().SetPlayerClaim(1, "subject-xyz");
    }

    [Test]
    public async Task FallsBackToUnknownWhenNoClaimsPresent()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject(Arg.Any<string>()).Returns((Player?)null);
        _host.PlayersRepository.Create(Arg.Any<Player>()).Returns(ci => ci.Arg<Player>());

        using var client = _host.CreateClient(TestJwt.WithAccessRole(subject: null));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.PlayersRepository.Received()
            .Create(Arg.Is<Player>(p => p.DisplayName == "Unknown"));
    }

    [Test]
    public async Task UseExistingPlayerWhenOneAlreadyExists()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject("test-user")
            .Returns(new Player("test-user", "Existing"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.PlayersRepository.DidNotReceive().Create(Arg.Any<Player>());
        _host.TeamsRepository.Received().SetPlayerClaim(1, "test-user");
    }

    [Test]
    public async Task Return409WhenTeamClaimedByAnotherUser_OnClaim()
    {
        _host.TeamsRepository.GetById(1)
            .Returns(new Team(1, "M1", "T1", "Bob", "other-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        _host.PlayersRepository.DidNotReceive().Create(Arg.Any<Player>());
        _host.TeamsRepository.DidNotReceive().SetPlayerClaim(Arg.Any<int>(), Arg.Any<string?>());
        _host.RatingsCalculator.DidNotReceive().CalculateForTeam(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task ClearClaimWhenUnclaimedByOwner()
    {
        _host.TeamsRepository.GetById(1)
            .Returns(new Team(1, "M1", "T1", "Alice", "test-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, false, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.TeamsRepository.Received().SetPlayerClaim(1, null);
    }

    [Test]
    public async Task Return403WhenUnclaimingTeamClaimedByAnother()
    {
        _host.TeamsRepository.GetById(1)
            .Returns(new Team(1, "M1", "T1", "Bob", "other-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, false, null));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        _host.TeamsRepository.DidNotReceive().SetPlayerClaim(Arg.Any<int>(), Arg.Any<string?>());
        _host.RatingsCalculator.DidNotReceive().CalculateForTeam(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task TriggerRecalculationOnSuccessfulClaim()
    {
        _host.TeamsRepository.GetById(1).Returns(new Team(1, "M1", "T1", null, null));
        _host.PlayersRepository.GetByAuthSubject("test-user")
            .Returns(new Player("test-user", "Alice"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.RatingsCalculator.Received(1).CalculateForTeam("M1", "T1");
    }

    [Test]
    public async Task TriggerRecalculationOnSuccessfulUnclaim()
    {
        _host.TeamsRepository.GetById(1)
            .Returns(new Team(1, "M2", "T2", "Alice", "test-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, false, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.RatingsCalculator.Received(1).CalculateForTeam("M2", "T2");
    }
}
