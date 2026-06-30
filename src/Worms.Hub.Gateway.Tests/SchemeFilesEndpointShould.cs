using System.Diagnostics.CodeAnalysis;
using System.Net;
using NUnit.Framework;
using Shouldly;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class SchemeFilesEndpointShould
{
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
    public async Task ReturnSchemeFileStreamForKnownScheme()
    {
        await _host.FileSystem.File.WriteAllTextAsync(
            Path.Combine(_host.SchemesFolder, "redgate-version.txt"),
            "1.2.3");
        await _host.FileSystem.File.WriteAllBytesAsync(Path.Combine(_host.SchemesFolder, "redgate.wsc"), [0x00]);

        var response = await _client.GetAsync(new Uri("api/v1/files/schemes/redgate", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/zip");
        var disposition = response.Content.Headers.ContentDisposition;
        disposition.ShouldNotBeNull();
        var filename = disposition.FileNameStar ?? disposition.FileName;
        filename.ShouldNotBeNullOrEmpty();
        filename.ShouldContain("redgate.1.2.3.wsc");
    }

    [Test]
    public async Task Return404ForUnknownScheme()
    {
        var response = await _client.GetAsync(new Uri("api/v1/files/schemes/unknownscheme", UriKind.Relative));
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Return404WhenSchemeFileMissing()
    {
        // Version file present but no .wsc file
        await _host.FileSystem.File.WriteAllTextAsync(
            Path.Combine(_host.SchemesFolder, "redgate-version.txt"),
            "1.2.3");

        var response = await _client.GetAsync(new Uri("api/v1/files/schemes/redgate", UriKind.Relative));
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
