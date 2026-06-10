using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using NSubstitute;
using Shouldly;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class GamesAuthShould
{
    private static readonly Uri GamesUri = new("api/v1/games", UriKind.Relative);

    private GatewayTestHost _host = null!;

    [SetUp]
    public void SetUp()
    {
        _host = new GatewayTestHost();
        // Always return an empty list so auth tests that reach the endpoint get 200
        _host.GamesRepository.GetAll().Returns(Array.Empty<Game>());
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public async Task Return401WhenNoTokenIsSupplied()
    {
        using var client = _host.CreateClient();

        var response = await client.GetAsync(GamesUri);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return401WhenTokenIsInvalid()
    {
        using var client = _host.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-jwt");

        var response = await client.GetAsync(GamesUri);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return403WhenTokenLacksAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithoutAccessRole());

        var response = await client.GetAsync(GamesUri);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return200WhenTokenHasAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithAccessRole());

        var response = await client.GetAsync(GamesUri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
