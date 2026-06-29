using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using Shouldly;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class CliFilesAuthShould
{
    private static readonly Uri CliUri = new("api/v1/files/cli", UriKind.Relative);
    private static readonly Uri CliWindowsUri = new("api/v1/files/cli/windows", UriKind.Relative);

    private GatewayTestHost _host = null!;

    [SetUp]
    public void SetUp()
    {
        _host = new GatewayTestHost();
        // Seed a valid version.txt and a platform file so anonymous GET endpoints return 200
        File.WriteAllText(Path.Combine(_host.CliFolder, "version.txt"), "1.0.0");
        File.WriteAllBytes(Path.Combine(_host.CliFolder, "worms-cli-linux.tar.gz"), [0x00]);
        File.WriteAllBytes(Path.Combine(_host.CliFolder, "worms-cli-windows.zip"), [0x00]);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public async Task AllowAnonymousGetOfCliDetails()
    {
        using var client = _host.CreateClient();
        var response = await client.GetAsync(CliUri);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task AllowAnonymousGetOfCliPlatformFile()
    {
        using var client = _host.CreateClient();
        var response = await client.GetAsync(CliWindowsUri);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Return401WhenPostingWithoutToken()
    {
        using var client = _host.CreateClient();
        using var content = BuildUpload("windows", "1.0.0", [0x00], "cli.zip");
        var response = await client.PostAsync(CliUri, content);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return401WhenPostingWithInvalidToken()
    {
        using var client = _host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-jwt");
        using var content = BuildUpload("windows", "1.0.0", [0x00], "cli.zip");
        var response = await client.PostAsync(CliUri, content);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return403WhenPostingWithAccessButNotWriteCli()
    {
        using var client = _host.CreateClient(TestJwt.WithAccessRole());
        using var content = BuildUpload("windows", "1.0.0", [0x00], "cli.zip");
        var response = await client.PostAsync(CliUri, content);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return200WhenPostingWithWriteCli()
    {
        using var client = _host.CreateClient(TestJwt.WithCliWriteRole());
        using var content = BuildUpload("windows", "2.0.0", [0x00], "cli.zip");
        var response = await client.PostAsync(CliUri, content);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [SuppressMessage("Reliability", "CA2000", Justification = "MultipartFormDataContent disposes its children")]
    private static MultipartFormDataContent BuildUpload(string platform, string version, byte[] fileBytes, string fileName)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(platform), "Platform");
        content.Add(new StringContent(version), "Version");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "File", fileName);
        return content;
    }
}
