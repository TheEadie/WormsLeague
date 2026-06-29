using System.Diagnostics.CodeAnalysis;
using System.Net;
using NUnit.Framework;
using Shouldly;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class GameFilesEndpointShould
{
    private static readonly Uri GameUri = new("api/v1/files/game", UriKind.Relative);

    private GatewayTestHost _host = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _host = new GatewayTestHost();
        _client = _host.CreateClient(TestJwt.WithGameDownloadRole());
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _host.Dispose();
    }

    [Test]
    public async Task ReturnZipArchiveWhenGameFolderExists()
    {
        // ReSharper disable once UseUtf8StringLiteral
        File.WriteAllBytes(Path.Combine(_host.GameFolder, "game.exe"), [0x4D, 0x5A]);

        var response = await _client.GetAsync(GameUri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/zip");
        var disposition = response.Content.Headers.ContentDisposition;
        disposition.ShouldNotBeNull();
        var filename = disposition.FileNameStar ?? disposition.FileName;
        filename.ShouldNotBeNullOrEmpty();
        filename.ShouldContain("wa-game.zip");
    }

    [Test]
    public async Task Return404WhenGameFolderMissing()
    {
        Directory.Delete(_host.GameFolder, recursive: true);

        var response = await _client.GetAsync(GameUri);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
