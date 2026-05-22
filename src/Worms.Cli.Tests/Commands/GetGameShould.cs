using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class GetGameShould
{
    [TestCase("game")]
    [TestCase("games")]
    public async Task PrintSingleGameOutputForOneResult(string alias)
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """[{"id":"g1","status":"InProgress","hostMachine":"host-a"}]""");

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", alias);

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("wa://host-a");
    }

    [Test]
    public async Task PrintTableWhenMultipleGamesReturned()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """[{"id":"g1","status":"InProgress","hostMachine":"host-a"},{"id":"g2","status":"Complete","hostMachine":"host-b"}]""");

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", "game");

        exitCode.ShouldBe(0);
        var output = console.Output.ToString();
        output.ShouldContain("g1");
        output.ShouldContain("g2");
        output.ShouldContain("host-a");
        output.ShouldContain("host-b");
        output.ShouldContain("InProgress");
        output.ShouldContain("Complete");
        output.ShouldContain("ID");
        output.ShouldContain("HOST");
        output.ShouldContain("STATUS");
    }

    [Test]
    public async Task ExitZeroWhenNoGamesReturnedAndNoNameProvided()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(HttpStatusCode.OK, "[]");

        var exitCode = await host.Run("get", "game");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldAllBe(m => m.Level != LogLevel.Error);
    }

    [Test]
    public async Task ReturnNonZeroWhenNoGameMatchesASpecificName()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(HttpStatusCode.OK, "[]");

        var exitCode = await host.Run("get", "game", "nonexistent");

        exitCode.ShouldBe(1);
    }

    [Test]
    public async Task LogWarningWhenUnauthorized()
    {
        using var host = new TestHost();
        // Store tokens so AccessTokenRefreshService can attempt the HTTP refresh call.
        // Access token must be a valid JWT so Runner.Run can call GetAnonymisedUserId without throwing.
        host.Services.GetRequiredService<ITokenStore>()
            .StoreAccessTokens(new AccessTokens(TestJwt.Create("test-user"), "fake-refresh"));
        host.Http.EnqueueAlways(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var exitCode = await host.Run("get", "game");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldContain(m =>
            m.Level == LogLevel.Warning &&
            m.Message.Contains("You don't have access to the Worms Hub"));
    }
}
