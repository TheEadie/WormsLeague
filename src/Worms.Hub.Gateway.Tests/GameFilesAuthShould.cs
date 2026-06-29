using System.Diagnostics.CodeAnalysis;
using System.Net;
using NUnit.Framework;
using Shouldly;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class GameFilesAuthShould
{
    private static readonly Uri GameUri = new("api/v1/files/game", UriKind.Relative);

    private GatewayTestHost _host = null!;

    [SetUp]
    public void SetUp() => _host = new GatewayTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public async Task Return401WhenNoTokenIsSupplied()
    {
        using var client = _host.CreateClient();
        var response = await client.GetAsync(GameUri);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return403WhenTokenLacksAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithoutAccessRole());
        var response = await client.GetAsync(GameUri);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return403WhenTokenHasAccessButNotDownloadGame()
    {
        using var client = _host.CreateClient(TestJwt.WithAccessRole());
        var response = await client.GetAsync(GameUri);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return200WhenTokenHasDownloadGame()
    {
        // Seed a file so the folder zips successfully
        File.WriteAllBytes(Path.Combine(_host.GameFolder, "game.exe"), [0x00]);

        using var client = _host.CreateClient(TestJwt.WithGameDownloadRole());
        var response = await client.GetAsync(GameUri);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
