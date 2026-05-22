using System.Net;
using NUnit.Framework;
using Shouldly;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class UpdateShould
{
    private static string LatestCliJson(string version) =>
        $$$"""{"latestVersion":"{{{version}}}","fileLocations":{}}""";

    [Test]
    public async Task LogUpToDateWhenCurrentVersionEqualsLatest()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(HttpStatusCode.OK, LatestCliJson("1.0.0"));

        var exitCode = await host.Run("update");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("Worms CLI is up to date"));
        host.CliUpdateDownloader.Calls.ShouldBeEmpty();
    }

    [Test]
    public async Task DownloadAndInstallWhenUpdateAvailable()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(HttpStatusCode.OK, LatestCliJson("2.0.0"));

        var exitCode = await host.Run("update");

        exitCode.ShouldBe(0);
        host.CliUpdateDownloader.Calls.Count.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("Update complete"));
    }

    [Test]
    public async Task DownloadAndInstallWhenForceFlagSet()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(HttpStatusCode.OK, LatestCliJson("1.0.0"));

        var exitCode = await host.Run("update", "--force");

        exitCode.ShouldBe(0);
        host.CliUpdateDownloader.Calls.Count.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("Update complete"));
    }

    [Test]
    public async Task ExitWithCodeOneWhenApiReturnsServerError()
    {
        using var host = new TestHost();
        // The API retries up to 3 times before throwing — enqueue a response for each attempt.
        host.Http.EnqueueResponse(HttpStatusCode.InternalServerError, "");
        host.Http.EnqueueResponse(HttpStatusCode.InternalServerError, "");
        host.Http.EnqueueResponse(HttpStatusCode.InternalServerError, "");

        var exitCode = await host.Run("update");

        exitCode.ShouldBe(1);
    }
}
