using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Resources.Remote;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Tests.Remote;

[TestFixture]
internal sealed class WormsServerApiShould
{
    [Test]
    public async Task RefreshAccessTokenAndRetryRequestOn401()
    {
        using var host = new TestHost();
        host.Services.GetRequiredService<ITokenStore>()
            .StoreAccessTokens(new AccessTokens("old-access", "old-refresh"));

        host.Http.EnqueueResponse(HttpStatusCode.Unauthorized, "");
        host.Http.EnqueueSuccessfulTokenResponse("new-access", "new-refresh");
        host.Http.EnqueueResponse(
            HttpStatusCode.OK,
            """{"latestVersion":"1.2.3","fileLocations":{"linux":"https://example/linux"}}""");

        var api = host.Services.GetRequiredService<IWormsServerApi>();

        var result = await api.GetLatestCliDetails();

        result.LatestVersion.ToString(3).ShouldBe("1.2.3");
        host.Http.Requests.Count.ShouldBe(3);

        host.Http.Requests[0].RequestUri!.AbsolutePath.ShouldBe("/api/v1/files/cli");
        host.Http.Requests[0].Authorization!.Parameter.ShouldBe("old-access");

        var refreshRequest = host.Http.Requests[1];
        refreshRequest.RequestUri!.AbsoluteUri.ShouldBe("https://eadie.eu.auth0.com/oauth/token");
        refreshRequest.Body.ShouldNotBeNull();
        refreshRequest.Body!.ShouldContain("grant_type=refresh_token");
        refreshRequest.Body.ShouldContain("refresh_token=old-refresh");
        refreshRequest.Body.ShouldContain("client_id=0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB");

        host.Http.Requests[2].RequestUri!.AbsolutePath.ShouldBe("/api/v1/files/cli");
        host.Http.Requests[2].Authorization!.Parameter.ShouldBe("new-access");
    }

    [Test]
    public async Task ThrowWhenRefreshIsTriggeredWithoutARefreshToken()
    {
        using var host = new TestHost();
        host.Services.GetRequiredService<ITokenStore>()
            .StoreAccessTokens(new AccessTokens("old-access", null));

        host.Http.EnqueueResponse(HttpStatusCode.Unauthorized, "");

        var api = host.Services.GetRequiredService<IWormsServerApi>();

        await Should.ThrowAsync<InvalidOperationException>(api.GetLatestCliDetails());
    }
}
