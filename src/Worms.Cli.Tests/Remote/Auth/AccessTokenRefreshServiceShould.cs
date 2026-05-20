using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Tests.Remote.Auth;

[TestFixture]
internal sealed class AccessTokenRefreshServiceShould
{
    [Test]
    public async Task ReturnTokensWithRefreshedAccessToken()
    {
        using var host = new TestHost();
        host.Http.EnqueueSuccessfulTokenResponse("new-access", "ignored-refresh");
        var service = host.Services.GetRequiredService<IAccessTokenRefreshService>();

        var result = await service.RefreshAccessTokens(new AccessTokens("old-access", "old-refresh"));

        result.AccessToken.ShouldBe("new-access");
        result.RefreshToken.ShouldBe("old-refresh");
        var request = host.Http.Requests.ShouldHaveSingleItem();
        request.RequestUri!.AbsoluteUri.ShouldBe("https://eadie.eu.auth0.com/oauth/token");
        request.Body.ShouldNotBeNull();
        request.Body!.ShouldContain("grant_type=refresh_token");
        request.Body.ShouldContain("refresh_token=old-refresh");
        request.Body.ShouldContain("client_id=0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB");
    }

    [Test]
    public async Task ThrowWhenRefreshTokenIsNull()
    {
        using var host = new TestHost();
        var service = host.Services.GetRequiredService<IAccessTokenRefreshService>();

        await Should.ThrowAsync<InvalidOperationException>(
            service.RefreshAccessTokens(new AccessTokens("old-access", null)));
    }
}
