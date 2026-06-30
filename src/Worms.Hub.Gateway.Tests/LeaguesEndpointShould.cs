using System.Diagnostics.CodeAnalysis;
using System.Net;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class LeaguesEndpointShould
{
    private const string LeaguesUrl = "api/v1/leagues";

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

    private void WriteVersionFile(string leagueId, string version) =>
        _host.FileSystem.File.WriteAllText(Path.Combine(_host.SchemesFolder, $"{leagueId}-version.txt"), version);

    // GET /api/v1/leagues

    [Test]
    public async Task ReturnEmptyListWhenNoLeaguesExist()
    {
        var response = await _client.GetAsync(new Uri(LeaguesUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var leagues = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<LeagueDto>>();
        leagues.ShouldNotBeNull();
        leagues.ShouldBeEmpty();
    }

    [Test]
    public async Task ReturnAllLeaguesWithStandings()
    {
        _host.Storage.Leagues.Seed(
            new LeagueDb("redgate", "Red Gate"),
            new LeagueDb("other", "Other"));
        _host.Storage.Ratings.Seed("redgate",
            new PlayerRating("sub1", "Alice", "redgate", 1500, 10),
            new PlayerRating("sub2", "Bob", "redgate", 1400, 8));

        var response = await _client.GetAsync(new Uri(LeaguesUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var leagues = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<LeagueDto>>();
        leagues.ShouldNotBeNull();
        leagues.Count.ShouldBe(2);
        var redgate = leagues.FirstOrDefault(l => l.Id == "redgate");
        redgate.ShouldNotBeNull();
        redgate.Name.ShouldBe("Red Gate");
        redgate.Standings.Count.ShouldBe(2);
        redgate.Standings.ShouldContain(s => s.PlayerName == "Alice" && s.Elo == 1500 && s.GamesPlayed == 10);
        redgate.Standings.ShouldContain(s => s.PlayerName == "Bob" && s.Elo == 1400 && s.GamesPlayed == 8);
    }

    [Test]
    public async Task ReturnNullVersionAndSchemeUrlWhenNoVersionFileExists()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));

        var response = await _client.GetAsync(new Uri(LeaguesUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var leagues = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<LeagueDto>>();
        leagues.ShouldNotBeNull();
        var redgate = leagues.FirstOrDefault(l => l.Id == "redgate");
        redgate.ShouldNotBeNull();
        redgate.Version.ShouldBeNull();
        redgate.SchemeUrl.ShouldBeNull();
    }

    [Test]
    public async Task ReturnVersionAndSchemeUrlWhenVersionFileExists()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));
        WriteVersionFile("redgate", "1.2.3");

        var response = await _client.GetAsync(new Uri(LeaguesUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var leagues = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<LeagueDto>>();
        leagues.ShouldNotBeNull();
        var redgate = leagues.FirstOrDefault(l => l.Id == "redgate");
        redgate.ShouldNotBeNull();
        redgate.Version.ShouldNotBeNull();
        redgate.Version!.ToString().ShouldBe("1.2.3");
        redgate.SchemeUrl.ShouldNotBeNull();
        redgate.SchemeUrl!.ToString().ShouldEndWith("api/v1/files/schemes/redgate");
    }

    // GET /api/v1/leagues/{id}

    [Test]
    public async Task ReturnLeagueByIdWhenItExists()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));
        _host.Storage.Ratings.Seed("redgate",
            new PlayerRating("sub1", "Alice", "redgate", 1500, 10));
        WriteVersionFile("redgate", "1.0.0");

        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/redgate", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<LeagueDto>();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe("redgate");
        dto.Name.ShouldBe("Red Gate");
        dto.Standings.Count.ShouldBe(1);
        dto.Standings[0].PlayerName.ShouldBe("Alice");
        dto.Version.ShouldNotBeNull();
        dto.SchemeUrl.ShouldNotBeNull();
    }

    [Test]
    public async Task Return404WhenLeagueByIdDoesNotExist()
    {
        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/missing", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // GET /api/v1/leagues/{id}/replays

    [Test]
    public async Task ReturnEmptyReplaysWhenLeagueHasNone()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));

        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/redgate/replays", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var replays = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ReplayDetailDto>>();
        replays.ShouldNotBeNull();
        replays.ShouldBeEmpty();
    }

    [Test]
    public async Task ReturnReplaysWhenLeagueHasThem()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));
        _host.Storage.Replays.Seed(
            new Replay("99", "My Game", "Processed", "file.dat", null, "redgate", null, null, null, null));

        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/redgate/replays", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var replays = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ReplayDetailDto>>();
        replays.ShouldNotBeNull();
        replays.Count.ShouldBe(1);
        var replay = replays.First();
        replay.Id.ShouldBe("99");
        replay.Name.ShouldBe("My Game");
        replay.Status.ShouldBe("Processed");
    }

    [Test]
    public async Task Return404ForReplaysWhenLeagueIdIsUnknown()
    {
        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/missing/replays", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // GET /api/v1/leagues/{id}/replays/{replayId}

    [Test]
    public async Task ReturnReplayWhenLeagueAndReplayExist()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));
        _host.Storage.Replays.Seed(
            new Replay("99", "My Game", "Processed", "file.dat", null, "redgate", null, null, null, null));

        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/redgate/replays/99", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ReplayDetailDto>();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe("99");
    }

    [Test]
    public async Task Return404ForReplayWhenLeagueIdIsUnknown()
    {
        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/missing/replays/99", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Return404ForReplayWhenReplayIdIsNotFound()
    {
        _host.Storage.Leagues.Seed(new LeagueDb("redgate", "Red Gate"));
        _host.Storage.Replays.Seed(
            new Replay("99", "My Game", "Processed", "file.dat", null, "redgate", null, null, null, null));

        var response = await _client.GetAsync(new Uri($"{LeaguesUrl}/redgate/replays/123", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
