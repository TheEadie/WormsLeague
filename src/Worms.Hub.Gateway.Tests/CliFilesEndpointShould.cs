using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Gateway.API.DTOs;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class CliFilesEndpointShould
{
    private static readonly Uri CliUri = new("api/v1/files/cli", UriKind.Relative);

    private GatewayTestHost _host = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _host = new GatewayTestHost();
        _client = _host.CreateClient(TestJwt.WithCliWriteRole());
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _host.Dispose();
    }

    [Test]
    public async Task ReturnLatestVersionAndPlatformsOnGet()
    {
        await _host.FileSystem.File.WriteAllTextAsync(Path.Combine(_host.CliFolder, "version.txt"), "1.2.3");
        await _host.FileSystem.File.WriteAllBytesAsync(
            Path.Combine(_host.CliFolder, "worms-cli-linux.tar.gz"),
            [0x00]);

        var response = await _client.GetAsync(CliUri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<CliFileDto>();
        dto.ShouldNotBeNull();
        dto.LatestVersion.ToString().ShouldBe("1.2.3");
        dto.FileLocations.ShouldContainKey("linux");
    }

    [Test]
    public async Task ReturnFileStreamForKnownPlatform()
    {
        await _host.FileSystem.File.WriteAllTextAsync(Path.Combine(_host.CliFolder, "version.txt"), "1.2.3");
        // ReSharper disable once UseUtf8StringLiteral
        await _host.FileSystem.File.WriteAllBytesAsync(
            Path.Combine(_host.CliFolder, "worms-cli-windows.zip"),
            [0x50, 0x4B]);

        var response = await _client.GetAsync(new Uri("api/v1/files/cli/windows", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/zip");
        var disposition = response.Content.Headers.ContentDisposition;
        disposition.ShouldNotBeNull();
        var filename = disposition.FileNameStar ?? disposition.FileName;
        filename.ShouldNotBeNullOrEmpty();
        filename.ShouldContain("worms-cli-windows.zip");
    }

    [Test]
    public async Task Return404ForUnknownPlatform()
    {
        // version.txt must exist because GetLatestDetails reads it before the platform check
        await _host.FileSystem.File.WriteAllTextAsync(Path.Combine(_host.CliFolder, "version.txt"), "1.0.0");

        var response = await _client.GetAsync(new Uri("api/v1/files/cli/unknownplatform", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Return404WhenPlatformFileMissing()
    {
        await _host.FileSystem.File.WriteAllTextAsync(Path.Combine(_host.CliFolder, "version.txt"), "1.0.0");
        await _host.FileSystem.File.WriteAllBytesAsync(
            Path.Combine(_host.CliFolder, "worms-cli-linux.tar.gz"),
            [0x00]);
        // No windows file

        var response = await _client.GetAsync(new Uri("api/v1/files/cli/windows", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AcceptValidUploadAndReflectNewVersion()
    {
        // ReSharper disable once UseUtf8StringLiteral
        using var content = BuildUpload("windows", "2.0.0", [0x50, 0x4B], "cli.zip");
        var response = await _client.PostAsync(CliUri, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<CliFileDto>();
        dto.ShouldNotBeNull();
        dto.LatestVersion.ToString().ShouldBe("2.0.0");

        // Subsequent GET should reflect the same version
        var getResponse = await _client.GetAsync(CliUri);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var getDto = await getResponse.Content.ReadFromJsonAsync<CliFileDto>();
        getDto.ShouldNotBeNull();
        getDto.LatestVersion.ToString().ShouldBe("2.0.0");
    }

    [Test]
    public async Task Return400ForUnknownPlatformOnUpload()
    {
        using var content = BuildUpload("unknownplatform", "1.0.0", [0x00], "cli.zip");
        var response = await _client.PostAsync(CliUri, content);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Return400ForUnsupportedExtensionOnUpload()
    {
        using var content = BuildUpload("windows", "1.0.0", [0x00], "cli.txt");
        var response = await _client.PostAsync(CliUri, content);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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
