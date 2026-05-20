using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Auth.Responses;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class AuthShould
{
    [TestCase("auth")]
    [TestCase("login")]
    public async Task LogInThroughDeviceCodeFlowAndPersistTokens(string commandName)
    {
        using var host = new TestHost();
        host.Http.EnqueueDeviceCodeResponse(
            deviceCode: "device-code-xyz",
            verificationUriComplete: "https://eadie.eu.auth0.com/activate?user_code=ABCD-EFGH");
        host.Http.EnqueueSuccessfulTokenResponse("access-token-1", "refresh-token-1");

        var exitCode = await host.Run(commandName);

        exitCode.ShouldBe(0);

        host.Browser.OpenedUrls.ShouldHaveSingleItem()
            .ShouldBe("https://eadie.eu.auth0.com/activate?user_code=ABCD-EFGH");

        host.Http.Requests.Count.ShouldBe(2);

        var deviceCodeRequest = host.Http.Requests[0];
        deviceCodeRequest.Method.ShouldBe(HttpMethod.Post);
        deviceCodeRequest.RequestUri!.AbsoluteUri.ShouldBe("https://eadie.eu.auth0.com/oauth/device/code");
        deviceCodeRequest.Body.ShouldNotBeNull();
        deviceCodeRequest.Body!.ShouldContain("client_id=0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB");
        deviceCodeRequest.Body.ShouldContain("scope=openid+profile+offline_access");
        deviceCodeRequest.Body.ShouldContain("audience=worms.davideadie.dev");

        var tokenRequest = host.Http.Requests[1];
        tokenRequest.Method.ShouldBe(HttpMethod.Post);
        tokenRequest.RequestUri!.AbsoluteUri.ShouldBe("https://eadie.eu.auth0.com/oauth/token");
        tokenRequest.Body.ShouldNotBeNull();
        tokenRequest.Body!.ShouldContain("grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Adevice_code");
        tokenRequest.Body.ShouldContain("device_code=device-code-xyz");
        tokenRequest.Body.ShouldContain("client_id=0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB");

        var stored = host.Services.GetRequiredService<ITokenStore>().GetAccessTokens();
        stored.AccessToken.ShouldBe("access-token-1");
        stored.RefreshToken.ShouldBe("refresh-token-1");
    }

    [Test]
    public async Task PollUntilAuthorizationPendingClears()
    {
        using var host = new TestHost();
        const int intervalSeconds = 5;
        host.Http.EnqueueDeviceCodeResponse(intervalSeconds: intervalSeconds);
        host.Http.EnqueueAuthorizationPending();
        host.Http.EnqueueSlowDown();
        host.Http.EnqueueSuccessfulTokenResponse("polled-access", "polled-refresh");

        var runTask = host.Run("auth");

        await host.Http.WaitForRequestCount(2);
        host.Time.Advance(TimeSpan.FromSeconds(intervalSeconds));
        await host.Http.WaitForRequestCount(3);
        host.Time.Advance(TimeSpan.FromSeconds(intervalSeconds));
        var exitCode = await runTask;

        exitCode.ShouldBe(0);
        host.Http.Requests.Count.ShouldBe(4);
        var stored = host.Services.GetRequiredService<ITokenStore>().GetAccessTokens();
        stored.AccessToken.ShouldBe("polled-access");
    }

    [Test]
    public async Task ReturnNonZeroAndNotPersistTokensWhenDeviceCodeRequestFails()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(System.Net.HttpStatusCode.InternalServerError, "server_error");

        var exitCode = await host.Run("auth");

        exitCode.ShouldBe(1);
        var stored = host.Services.GetRequiredService<ITokenStore>().GetAccessTokens();
        stored.AccessToken.ShouldBeNull();
        stored.RefreshToken.ShouldBeNull();
    }

    [Test]
    public async Task ThrowHttpRequestExceptionDirectlyWhenDeviceCodeRequestFails()
    {
        using var host = new TestHost();
        host.Http.EnqueueResponse(System.Net.HttpStatusCode.InternalServerError, "server_error");
        var login = host.Services.GetRequiredService<ILoginService>();

        await Should.ThrowAsync<HttpRequestException>(login.RequestLogin(CancellationToken.None));
    }

    [Test]
    public async Task ReturnNonZeroAndNotPersistTokensWhenTokenPollReturnsUnknownError()
    {
        using var host = new TestHost();
        host.Http.EnqueueDeviceCodeResponse();
        host.Http.EnqueueResponse(System.Net.HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");

        var exitCode = await host.Run("auth");

        exitCode.ShouldBe(1);
        var stored = host.Services.GetRequiredService<ITokenStore>().GetAccessTokens();
        stored.AccessToken.ShouldBeNull();
    }

    [Test]
    public async Task ThrowHttpRequestExceptionDirectlyWhenTokenPollReturnsUnknownError()
    {
        using var host = new TestHost();
        host.Http.EnqueueDeviceCodeResponse();
        host.Http.EnqueueResponse(System.Net.HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");
        var login = host.Services.GetRequiredService<ILoginService>();

        await Should.ThrowAsync<HttpRequestException>(login.RequestLogin(CancellationToken.None));
    }

    [Test]
    public async Task NotPersistTokensWhenDeviceCodeTimesOut()
    {
        using var host = new TestHost();
        const int expiresInSeconds = 30;
        const int intervalSeconds = 5;
        host.Http.EnqueueDeviceCodeResponse(
            expiresInSeconds: expiresInSeconds,
            intervalSeconds: intervalSeconds);
        host.Http.EnqueueAlways(_ => new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        {
            Content = new StringContent("""{"error":"authorization_pending"}""")
        });

        var runTask = host.Run("auth");
        await host.Http.WaitForRequestCount(2);

        // Walk virtual time forward one polling interval at a time. The timeout CTS
        // (set to expiresInSeconds) cancels the loop mid-await once that elapses,
        // throwing OperationCanceledException which Runner.Run maps to exit code 1.
        while (!runTask.IsCompleted)
        {
            var beforeAdvance = host.Http.Requests.Count;
            host.Time.Advance(TimeSpan.FromSeconds(intervalSeconds));
            await Task.WhenAny(host.Http.WaitForRequestCount(beforeAdvance + 1), runTask);
        }

        var exitCode = await runTask;
        exitCode.ShouldBe(1);
        var stored = host.Services.GetRequiredService<ITokenStore>().GetAccessTokens();
        stored.AccessToken.ShouldBeNull();
        stored.RefreshToken.ShouldBeNull();
    }
}
