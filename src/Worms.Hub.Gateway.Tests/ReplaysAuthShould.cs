using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class ReplaysAuthShould
{
    private static readonly Uri ReplaysUri = new("api/v1/replays", UriKind.Relative);

    // A minimal valid replay: correct .wagame extension, WA signature
    private static readonly byte[] ValidReplayBytes = BuildValidReplayBytes();

    private static byte[] BuildValidReplayBytes()
    {
        var bytes = new byte[64];
        bytes[0] = (byte)'W';
        bytes[1] = (byte)'A';
        return bytes;
    }

    private GatewayTestHost _host = null!;

    [SetUp]
    public void SetUp() => _host = new GatewayTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public async Task Return401WhenNoTokenIsSupplied()
    {
        using var client = _host.CreateClient();
        using var content = BuildEmptyForm();

        var response = await client.PostAsync(ReplaysUri, content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return401WhenTokenIsInvalid()
    {
        using var client = _host.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-jwt");
        using var content = BuildEmptyForm();

        var response = await client.PostAsync(ReplaysUri, content);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Return403WhenTokenLacksAccessRole()
    {
        using var client = _host.CreateClient(TestJwt.WithoutAccessRole());
        using var content = BuildEmptyForm();

        var response = await client.PostAsync(ReplaysUri, content);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Return200WhenTokenHasAccessRole()
    {
        var created = new Replay("1", "Game", "Pending", "file.dat", null, "redgate", null, null, null, null);
        _host.ReplaysRepository.Create(Arg.Any<Replay>()).Returns(created);

        using var client = _host.CreateClient(TestJwt.WithAccessRole());
        using var content = BuildUpload("Game", ValidReplayBytes, "game.WAGame");

        var response = await client.PostAsync(ReplaysUri, content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static MultipartFormDataContent BuildEmptyForm() => new();

    [SuppressMessage("Reliability", "CA2000", Justification = "MultipartFormDataContent disposes its children")]
    private static MultipartFormDataContent BuildUpload(string name, byte[] fileBytes, string fileName)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "Name");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "ReplayFile", fileName);
        return content;
    }
}
