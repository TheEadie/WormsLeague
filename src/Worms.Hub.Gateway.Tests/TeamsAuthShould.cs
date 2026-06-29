using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using Shouldly;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class TeamsAuthShould
{
    private static readonly Uri TeamsUri = new("api/v1/teams", UriKind.Relative);

    private GatewayTestHost _host = null!;

    [SetUp]
    public void SetUp()
    {
        _host = new GatewayTestHost();
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public async Task Return401WhenNoTokenIsSupplied()
    {
        using var client = _host.CreateClient();

        var response = await client.GetAsync(TeamsUri);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return401WhenTokenIsInvalid()
    {
        using var client = _host.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-jwt");

        var response = await client.GetAsync(TeamsUri);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return403WhenTokenLacksAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithoutAccessRole());

        var response = await client.GetAsync(TeamsUri);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return200WhenTokenHasAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithAccessRole());

        var response = await client.GetAsync(TeamsUri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
