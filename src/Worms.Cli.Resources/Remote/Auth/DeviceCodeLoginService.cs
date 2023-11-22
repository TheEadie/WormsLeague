using System.Text.Json;
using Serilog;
using Worms.Cli.Resources.Remote.Auth.Responses;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class DeviceCodeLoginService(ITokenStore tokenStore, IHttpClientFactory httpClientFactory)
    : ILoginService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";
    private const string Audience = "worms.davideadie.dev";

    public async Task RequestLogin(ILogger logger, CancellationToken cancellationToken)
    {
        logger.Verbose("Requesting device code...");
        var deviceCodeResponse = await RequestDeviceCode(logger, cancellationToken).ConfigureAwait(false);
        logger.Information(
            $"Please visit {deviceCodeResponse.VerificationUri} and enter the code: {deviceCodeResponse.UserCode}");

        logger.Verbose("Opening browser...");
        BrowserLauncher.OpenBrowser(deviceCodeResponse.VerificationUriComplete.OriginalString);

        logger.Verbose("Requesting tokens...");
        var tokenResponse =
            await RequestTokenAsync(deviceCodeResponse, logger, cancellationToken).ConfigureAwait(false);

        if (tokenResponse != null)
        {
            logger.Verbose("Saving tokens...");
            tokenStore.StoreAccessTokens(new AccessTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken));

            logger.Information("Logged in successfully");
            return;
        }

        logger.Error("Error logging in");
    }

    private async Task<DeviceAuthorizationResponse> RequestDeviceCode(
        ILogger logger,
        CancellationToken cancellationToken)
    {
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
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var streamAsync = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var stream = streamAsync.ConfigureAwait(false);
            var jsonResponse = await JsonSerializer.DeserializeAsync(
                    streamAsync,
                    JsonContext.Default.DeviceAuthorizationResponse,
                    cancellationToken)
                .ConfigureAwait(false);

            if (jsonResponse is null)
            {
                logger.Error("Error requesting device code: No response content");
                throw new HttpRequestException("No response content");
            }

            return jsonResponse;
        }

        var stringContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.Error($"Error requesting device code: {stringContent}");
        throw new HttpRequestException(stringContent);
    }


    private async Task<TokenResponse?> RequestTokenAsync(
        DeviceAuthorizationResponse deviceCodeResponse,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var timeout = TimeSpan.FromSeconds(deviceCodeResponse.ExpiresIn);
        cancellationTokenSource.CancelAfter(timeout);

        logger.Verbose($"Checking if code has been confirmed... (Timeout: {timeout.TotalMinutes} mins)");

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
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var streamAsync = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await using var stream = streamAsync.ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync(
                        streamAsync,
                        JsonContext.Default.TokenResponse,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var stringContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (stringContent.Contains("authorization_pending", StringComparison.InvariantCulture)
                || stringContent.Contains("slow_down", StringComparison.InvariantCulture))
            {
                logger.Verbose($"Code not yet confirmed. Retrying in {deviceCodeResponse.Interval} seconds");
                await Task.Delay(deviceCodeResponse.Interval * 1000, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                logger.Error($"Error logging in: {stringContent}");
                throw new HttpRequestException(stringContent);
            }
        }

        logger.Warning($"Requesting tokens timed out after ${timeout} seconds");
        return null;
    }
}
