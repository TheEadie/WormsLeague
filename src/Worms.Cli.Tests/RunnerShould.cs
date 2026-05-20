using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests;

[TestFixture]
internal sealed class RunnerShould
{
    [Test]
    public async Task LogLoggedInMessageWhenTokensExist()
    {
        using var host = new TestHost();
        host.Services.GetRequiredService<ITokenStore>()
            .StoreAccessTokens(new AccessTokens(TestJwt.Create("test|abc123"), "refresh-token"));

        var exitCode = await host.Run("version");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldContain(m =>
            m.Level == LogLevel.Debug &&
            m.Message.Contains("Logged in to Worms Hub as test|abc123"));
    }

    [Test]
    public async Task LogNotLoggedInMessageWhenNoTokens()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("version");

        exitCode.ShouldBe(0);
        host.Logs.Messages.ShouldContain(m =>
            m.Level == LogLevel.Debug &&
            m.Message.Contains("Not logged in to Worms Hub"));
    }
}
