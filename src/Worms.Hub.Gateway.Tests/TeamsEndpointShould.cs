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
        var response = await _client.GetAsync(new Uri(TeamsUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var teams = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TeamDto>>();
        teams.ShouldNotBeNull();
        teams.ShouldBeEmpty();
    }

    [Test]
    public async Task ReturnAllTeamsMappingFields()
    {
        _host.Storage.Teams.Seed(
            new Team(1, "Machine1", "TeamA", null, null),
            new Team(2, "Machine2", "TeamB", "Alice", "test-user"),
            new Team(3, "Machine3", "TeamC", "Bob", "other-user"));

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
        _host.Storage.Teams.Seed(
            new Team(1, "M1", "T1", null, null),
            new Team(2, "M2", "T2", "Alice", "test-user"),
            new Team(3, "M3", "T3", "Bob", "other-user"));

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
        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(999, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        _host.Storage.Players.GetByAuthSubject("test-user").ShouldBeNull();
        _host.Storage.Teams.GetById(999).ShouldBeNull();
        _host.RatingsCalculator.DidNotReceive().CalculateForTeam(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task CreatePlayerAndClaimWhenUnclaimedAndNoPlayerExists_BodyDisplayName()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, "Body Name"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var player = _host.Storage.Players.GetByAuthSubject("test-user");
        player.ShouldNotBeNull();
        player.DisplayName.ShouldBe("Body Name");
        _host.Storage.Teams.GetById(1)!.ClaimedByAuthSubject.ShouldBe("test-user");
    }

    [Test]
    public async Task FallsBackToNicknameClaim()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));

        using var client = _host.CreateClient(TestJwt.WithAccessRole(nickname: "NickFromToken"));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var player = _host.Storage.Players.GetByAuthSubject("test-user");
        player.ShouldNotBeNull();
        player.DisplayName.ShouldBe("NickFromToken");
    }

    [Test]
    public async Task FallsBackToNameClaimWhenNoNickname()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));

        using var client = _host.CreateClient(TestJwt.WithAccessRole(name: "NameFromToken"));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var player = _host.Storage.Players.GetByAuthSubject("test-user");
        player.ShouldNotBeNull();
        player.DisplayName.ShouldBe("NameFromToken");
    }

    [Test]
    public async Task FallsBackToSubjectWhenNoNicknameOrName()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));

        using var client = _host.CreateClient(TestJwt.WithAccessRole(subject: "subject-xyz"));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var player = _host.Storage.Players.GetByAuthSubject("subject-xyz");
        player.ShouldNotBeNull();
        player.DisplayName.ShouldBe("subject-xyz");
        _host.Storage.Teams.GetById(1)!.ClaimedByAuthSubject.ShouldBe("subject-xyz");
    }

    [Test]
    public async Task FallsBackToUnknownWhenNoClaimsPresent()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));

        using var client = _host.CreateClient(TestJwt.WithAccessRole(subject: null));

        var response = await client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        // Assert via snapshot — subject is null so we can't key into the player store
        var allPlayers = _host.Storage.Players.All;
        allPlayers.Count.ShouldBe(1);
        allPlayers.Single().DisplayName.ShouldBe("Unknown");
    }

    [Test]
    public async Task UseExistingPlayerWhenOneAlreadyExists()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));
        _host.Storage.Players.Seed(new Player("test-user", "Existing"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.Storage.Players.All.Count.ShouldBe(1);
        _host.Storage.Players.All.Single().DisplayName.ShouldBe("Existing");
        _host.Storage.Teams.GetById(1)!.ClaimedByAuthSubject.ShouldBe("test-user");
    }

    [Test]
    public async Task Return409WhenTeamClaimedByAnotherUser_OnClaim()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", "Bob", "other-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        _host.Storage.Players.All.ShouldBeEmpty();
        _host.Storage.Teams.GetById(1)!.ClaimedByAuthSubject.ShouldBe("other-user");
        _host.RatingsCalculator.DidNotReceive().CalculateForTeam(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task ClearClaimWhenUnclaimedByOwner()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", "Alice", "test-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, false, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.Storage.Teams.GetById(1)!.ClaimedByAuthSubject.ShouldBeNull();
    }

    [Test]
    public async Task Return403WhenUnclaimingTeamClaimedByAnother()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", "Bob", "other-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, false, null));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        _host.Storage.Teams.GetById(1)!.ClaimedByAuthSubject.ShouldBe("other-user");
        _host.RatingsCalculator.DidNotReceive().CalculateForTeam(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task TriggerRecalculationOnSuccessfulClaim()
    {
        _host.Storage.Teams.Seed(new Team(1, "M1", "T1", null, null));
        _host.Storage.Players.Seed(new Player("test-user", "Alice"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, true, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.RatingsCalculator.Received(1).CalculateForTeam("M1", "T1");
    }

    [Test]
    public async Task TriggerRecalculationOnSuccessfulUnclaim()
    {
        _host.Storage.Teams.Seed(new Team(1, "M2", "T2", "Alice", "test-user"));

        var response = await _client.PutAsJsonAsync(TeamsUrl, new ClaimTeamDto(1, false, null));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.RatingsCalculator.Received(1).CalculateForTeam("M2", "T2");
    }
}
