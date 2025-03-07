using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources.Remote.Auth.Responses;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class DeviceCodeLoginService(
    ITokenStore tokenStore,
    IHttpClientFactory httpClientFactory,
    ILogger<DeviceCodeLoginService> logger) : ILoginService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";
    private const string Audience = "worms.davideadie.dev";

    public async Task RequestLogin(CancellationToken cancellationToken)
    {
        logger.LogDebug("Requesting device code...");
        var deviceCodeResponse = await RequestDeviceCode(cancellationToken);
        logger.LogInformation(
            "Please visit {VerificationUri} and enter the code: {UserCode}",
            deviceCodeResponse.VerificationUri,
            deviceCodeResponse.UserCode);

        logger.LogDebug("Opening browser...");
        BrowserLauncher.OpenBrowser(deviceCodeResponse.VerificationUriComplete.OriginalString);

        logger.LogDebug("Requesting tokens...");
        var tokenResponse =
            await RequestTokenAsync(deviceCodeResponse, logger, cancellationToken);

        if (tokenResponse != null)
        {
            logger.LogDebug("Saving tokens...");
            tokenStore.StoreAccessTokens(new AccessTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken));

            logger.LogInformation("Logged in successfully");
            return;
        }

        logger.LogError("Error logging in");
    }

    private async Task<DeviceAuthorizationResponse> RequestDeviceCode(CancellationToken cancellationToken)
    {
        using var span = Activity.Current?.Source.StartActivity(Telemetry.Spans.RequestDeviceCode.SpanName);
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(Authority);

        const string scopes = "openid profile offline_access";
        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>()
            {
                { "client_id", ClientId },
                { "scope", scopes },
                { "audience", Audience }
            });

        var response = await httpClient.PostAsync(
                new Uri("oauth/device/code", UriKind.Relative),
                content,
                cancellationToken)
            ;

        if (response.IsSuccessStatusCode)
        {
            var streamAsync = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var stream = streamAsync;
            var jsonResponse = await JsonSerializer.DeserializeAsync(
                    streamAsync,
                    JsonContext.Default.DeviceAuthorizationResponse,
                    cancellationToken)
                ;

            if (jsonResponse is null)
            {
                logger.LogError("Error requesting device code: No response content");
                throw new HttpRequestException("No response content");
            }

            return jsonResponse;
        }

        var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogError("Error requesting device code: {Error}", stringContent);
        throw new HttpRequestException(stringContent);
    }

    private async Task<TokenResponse?> RequestTokenAsync(
        DeviceAuthorizationResponse deviceCodeResponse,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using var span = Activity.Current?.Source.StartActivity(Telemetry.Spans.GetAuthTokens.SpanName);
        using var cancellationTokenSource = new CancellationTokenSource();
        var timeout = TimeSpan.FromSeconds(deviceCodeResponse.ExpiresIn);
        cancellationTokenSource.CancelAfter(timeout);

        logger.LogDebug(
            "Checking if code has been confirmed... (Timeout: {TimeoutMinutes} mins)",
            timeout.TotalMinutes);

        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Authority);

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var content = new FormUrlEncodedContent(
                new Dictionary<string, string>()
                {
                    { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                    { "device_code", deviceCodeResponse.DeviceCode },
                    { "client_id", ClientId }
                });

            var response = await client.PostAsync(new Uri("oauth/token", UriKind.Relative), content, cancellationToken)
                ;

            if (response.IsSuccessStatusCode)
            {
                var streamAsync = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var stream = streamAsync;
                return await JsonSerializer.DeserializeAsync(
                        streamAsync,
                        JsonContext.Default.TokenResponse,
                        cancellationToken)
                    ;
            }

            var stringContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (stringContent.Contains("authorization_pending", StringComparison.InvariantCulture)
                || stringContent.Contains("slow_down", StringComparison.InvariantCulture))
            {
                logger.LogDebug(
                    "Code not yet confirmed. Retrying in {IntervalSeconds} seconds",
                    deviceCodeResponse.Interval);
                await Task.Delay(deviceCodeResponse.Interval * 1000, cancellationToken);
            }
            else
            {
                logger.LogError("Error logging in: {Error}", stringContent);
                throw new HttpRequestException(stringContent);
            }
        }

        logger.LogWarning("Requesting tokens timed out after ${TimeoutSeconds} seconds", timeout);
        return null;
    }
}
