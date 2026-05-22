using System.Net;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Resources.Local.Network;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class HostShould
{
    private static void EnqueueHostHappyPath(TestHost host)
    {
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"redgate","name":"Redgate","version":"1.2.3","schemeUrl":"https://example/test.wsc"}""");
        host.Http.EnqueueResponse(HttpStatusCode.OK, "scheme-bytes", "application/octet-stream");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"InProgress","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"Complete","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"replay-1","name":"replay","status":"Uploaded"}""");
    }

    [Test]
    public async Task RunFullFlowWhenNoFlagsSet()
    {
        using var host = new TestHost();
        EnqueueHostHappyPath(host);

        var exitCode = await host.Run("host");

        exitCode.ShouldBe(0);
        host.WormsArmageddon.HostWasCalled.ShouldBeTrue();
        host.Http.Requests.Count.ShouldBe(5);
        host.Http.Requests[0].RequestUri!.AbsolutePath.ShouldEndWith("/api/v1/leagues/redgate");
        host.Http.Requests[0].Method.ShouldBe(HttpMethod.Get);
        host.Http.Requests[1].RequestUri!.AbsolutePath.ShouldEndWith("/api/v1/files/schemes/redgate");
        host.Http.Requests[1].Method.ShouldBe(HttpMethod.Get);
        host.Http.Requests[2].RequestUri!.AbsolutePath.ShouldEndWith("/api/v1/games");
        host.Http.Requests[2].Method.ShouldBe(HttpMethod.Post);
        host.Http.Requests[3].RequestUri!.AbsolutePath.ShouldEndWith("/api/v1/games");
        host.Http.Requests[3].Method.ShouldBe(HttpMethod.Put);
        host.Http.Requests[4].RequestUri!.AbsolutePath.ShouldEndWith("/api/v1/replays");
        host.Http.Requests[4].Method.ShouldBe(HttpMethod.Post);
    }

    [Test]
    public async Task PrintWhatWillHappenWhenDryRunSet()
    {
        using var host = new TestHost();

        var runTask = host.Run("host", "--dry-run");

        host.Time.Advance(TimeSpan.FromSeconds(6));
        var exitCode = await runTask;

        exitCode.ShouldBe(0);
        host.WormsArmageddon.HostWasCalled.ShouldBeFalse();
        host.Http.Requests.ShouldBeEmpty();
    }

    [Test]
    public async Task SkipSchemeDownloadWhenFlagSet()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"InProgress","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"Complete","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"replay-1","name":"replay","status":"Uploaded"}""");

        var exitCode = await host.Run("host", "--skip-scheme-download");

        exitCode.ShouldBe(0);
        host.WormsArmageddon.HostWasCalled.ShouldBeTrue();
        host.Http.Requests.Count.ShouldBe(3);
        host.Http.Requests.ShouldAllBe(r =>
            !r.RequestUri!.AbsolutePath.Contains("leagues") &&
            !r.RequestUri.AbsolutePath.Contains("files/schemes"));
    }

    [Test]
    public async Task SkipReplayUploadWhenSkipUploadSet()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"redgate","name":"Redgate","version":"1.2.3","schemeUrl":"https://example/test.wsc"}""");
        host.Http.EnqueueResponse(HttpStatusCode.OK, "scheme-bytes", "application/octet-stream");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"InProgress","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"Complete","hostMachine":"10.0.0.1"}""");

        var exitCode = await host.Run("host", "--skip-upload");

        exitCode.ShouldBe(0);
        host.WormsArmageddon.HostWasCalled.ShouldBeTrue();
        host.Http.Requests.ShouldAllBe(r => !r.RequestUri!.AbsolutePath.EndsWith("/api/v1/replays"));
    }

    [Test]
    public async Task SkipGameAnnouncementWhenSkipAnnouncementSet()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"redgate","name":"Redgate","version":"1.2.3","schemeUrl":"https://example/test.wsc"}""");
        host.Http.EnqueueResponse(HttpStatusCode.OK, "scheme-bytes", "application/octet-stream");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"replay-1","name":"replay","status":"Uploaded"}""");

        var exitCode = await host.Run("host", "--skip-announcement");

        exitCode.ShouldBe(0);
        host.WormsArmageddon.HostWasCalled.ShouldBeTrue();
        host.Http.Requests.ShouldAllBe(r => !r.RequestUri!.AbsolutePath.EndsWith("/api/v1/games"));
        host.Http.Requests.ShouldContain(r => r.RequestUri!.AbsolutePath.EndsWith("/api/v1/replays"));
    }

    [Test]
    public async Task ReturnNonZeroWhenWormsArmageddonNotInstalled()
    {
        using var host = new TestHost(wormsInstalled: false);

        var exitCode = await host.Run("host");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("Worms Armageddon is not installed"));
    }

    [Test]
    public async Task ReturnNonZeroWhenIpAddressNotFound()
    {
        using var host = new TestHost();
        host.IpAddressLookup.Result = new IpAddressNotFound("No network adapter found for domain: red-gate.com");

        var exitCode = await host.Run("host");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("No network adapter found for domain: red-gate.com"));
    }

    [Test]
    public async Task LogWarningAndSkipUploadWhenNoReplayExists()
    {
        using var host = new TestHost(hostCreatesReplay: false);
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"redgate","name":"Redgate","version":"1.2.3","schemeUrl":"https://example/test.wsc"}""");
        host.Http.EnqueueResponse(HttpStatusCode.OK, "scheme-bytes", "application/octet-stream");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"InProgress","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"Complete","hostMachine":"10.0.0.1"}""");

        var exitCode = await host.Run("host");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("No replay found to upload"));
        host.Http.Requests.ShouldAllBe(r => !r.RequestUri!.AbsolutePath.EndsWith("/api/v1/replays"));
    }

    [Test]
    public async Task LogWarningAndSkipUploadWhenOnlyOldReplayExists()
    {
        using var host = new TestHost(hostCreatesReplay: false);
        // Write a replay from 2024 — it is always more than 1 hour old relative to the current date.
        ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");

        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"redgate","name":"Redgate","version":"1.2.3","schemeUrl":"https://example/test.wsc"}""");
        host.Http.EnqueueResponse(HttpStatusCode.OK, "scheme-bytes", "application/octet-stream");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"InProgress","hostMachine":"10.0.0.1"}""");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"id":"game-1","status":"Complete","hostMachine":"10.0.0.1"}""");

        var exitCode = await host.Run("host");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldContain(m => m.Level == LogLevel.Warning && m.Message.Contains("No recent replay found to upload"));
        host.Http.Requests.ShouldAllBe(r => !r.RequestUri!.AbsolutePath.EndsWith("/api/v1/replays"));
    }
}
