using System.Net;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class GamesEndpointShould
{
    private const string GamesUrl = "api/v1/games";

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

    [Test]
    public async Task ReturnEmptyListWhenNoGamesExist()
    {
        _host.GamesRepository.GetAll().Returns([]);

        var response = await _client.GetAsync(new Uri(GamesUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var games = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<GameDto>>();
        games.ShouldNotBeNull();
        games.ShouldBeEmpty();
    }

    [Test]
    public async Task ReturnAllGamesWhenRepositoryIsPopulated()
    {
        var gameA = new Game("1", "Pending", "HOST-A");
        var gameB = new Game("2", "Complete", "HOST-B");
        _host.GamesRepository.GetAll().Returns([gameA, gameB]);

        var response = await _client.GetAsync(new Uri(GamesUrl, UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var games = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<GameDto>>();
        games.ShouldNotBeNull();
        games.Count.ShouldBe(2);
        games.ShouldContain(g => g.Id == "1" && g.Status == "Pending" && g.HostMachine == "HOST-A");
        games.ShouldContain(g => g.Id == "2" && g.Status == "Complete" && g.HostMachine == "HOST-B");
    }

    [Test]
    public async Task ReturnGameByIdWhenItExists()
    {
        var game = new Game("42", "Pending", "HOST-1");
        _host.GamesRepository.GetAll().Returns([game]);

        var response = await _client.GetAsync(new Uri($"{GamesUrl}/42", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<GameDto>();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe("42");
        dto.Status.ShouldBe("Pending");
        dto.HostMachine.ShouldBe("HOST-1");
    }

    [Test]
    public async Task Return404WhenGameByIdDoesNotExist()
    {
        _host.GamesRepository.GetAll().Returns([]);

        var response = await _client.GetAsync(new Uri($"{GamesUrl}/999", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateGameAndAnnounceGameStarting()
    {
        var createdGame = new Game("42", "Pending", "HOST-1");
        _host.GamesRepository
            .Create(Arg.Any<Game>())
            .Returns(createdGame);

        var response = await _client.PostAsJsonAsync(GamesUrl, new CreateGameDto("HOST-1"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<GameDto>();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe("42");
        dto.Status.ShouldBe("Pending");
        dto.HostMachine.ShouldBe("HOST-1");

        await _host.Announcer.Received(1).AnnounceGameStarting("HOST-1");
        _host.GamesRepository.Received(1).Create(
            Arg.Is<Game>(g => g.HostMachine == "HOST-1" && g.Status == "Pending" && g.Id == "0"));
    }

    [Test]
    public async Task UpdateGameWhenItExists()
    {
        var existing = new Game("7", "Pending", "HOST-X");
        _host.GamesRepository.GetAll().Returns([existing]);

        var response = await _client.PutAsJsonAsync(GamesUrl, new GameDto("7", "Complete", "HOST-X"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _host.GamesRepository.Received(1).Update(Arg.Is<Game>(g => g.Id == "7"));
    }

    [Test]
    public async Task Return404WhenUpdatingGameThatDoesNotExist()
    {
        _host.GamesRepository.GetAll().Returns([]);

        var response = await _client.PutAsJsonAsync(GamesUrl, new GameDto("999", "Complete", "HOST-X"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        _host.GamesRepository.DidNotReceive().Update(Arg.Any<Game>());
    }
}
