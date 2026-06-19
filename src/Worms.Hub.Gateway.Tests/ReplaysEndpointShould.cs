using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Queues;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

[TestFixture]
[SuppressMessage("Design", "CA1001", Justification = "Disposed in TearDown")]
internal sealed class ReplaysEndpointShould
{
    private const string ReplaysUrl = "api/v1/replays";

    private GatewayTestHost _host = null!;
    private HttpClient _client = null!;

    // A minimal valid replay: correct .wagame extension, WA signature
    private static readonly byte[] ValidReplayBytes = BuildValidReplayBytes();

    private static byte[] BuildValidReplayBytes()
    {
        var bytes = new byte[64];
        bytes[0] = (byte)'W';
        bytes[1] = (byte)'A';
        return bytes;
    }

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
    public async Task AcceptAValidReplayUpload()
    {
        var created = new Replay("99", "My Game", "Pending", "temp-file.dat", null, "redgate", null, null, null, null);
        _host.ReplaysRepository.Create(Arg.Any<Replay>()).Returns(created);

        using var content = BuildUpload("My Game", ValidReplayBytes, "game.WAGame");
        var response = await _client.PostAsync(new Uri(ReplaysUrl, UriKind.Relative), content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<ReplayDto>();
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe("99");
        dto.Name.ShouldBe("My Game");
        dto.Status.ShouldBe("Pending");

        _host.ReplaysRepository.Received(1).Create(
            Arg.Is<Replay>(r =>
                r.Name == "My Game" &&
                r.Status == "Pending" &&
                r.Id == "0" &&
                r.LeagueId == "redgate"));

        await _host.ReplayProcessorQueue.Received(1).EnqueueMessage(
            Arg.Is<ReplayToProcessMessage>(m => m.ReplayFileName == created.Filename));
    }

    [Test]
    public async Task RejectAnUploadWithBadSignature()
    {
        using var content = BuildUpload("My Game", "\0\0\0"u8.ToArray(), "game.WAGame");
        var response = await _client.PostAsync(new Uri(ReplaysUrl, UriKind.Relative), content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        _host.ReplaysRepository.DidNotReceive().Create(Arg.Any<Replay>());
        await _host.ReplayProcessorQueue.DidNotReceive().EnqueueMessage(Arg.Any<ReplayToProcessMessage>());
    }

    [Test]
    public async Task RejectAnUploadWithTheWrongExtension()
    {
        using var content = BuildUpload("My Game", ValidReplayBytes, "game.txt");
        var response = await _client.PostAsync(new Uri(ReplaysUrl, UriKind.Relative), content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        _host.ReplaysRepository.DidNotReceive().Create(Arg.Any<Replay>());
        await _host.ReplayProcessorQueue.DidNotReceive().EnqueueMessage(Arg.Any<ReplayToProcessMessage>());
    }

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
