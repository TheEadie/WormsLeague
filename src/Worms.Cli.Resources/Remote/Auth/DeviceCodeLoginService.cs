using System.Text.Json;
using RestSharp;
using Serilog;
using Worms.Cli.Resources.Remote.Auth.Responses;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class DeviceCodeLoginService(ITokenStore tokenStore) : ILoginService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";
    private const string Audience = "worms.davideadie.dev";

    public async Task RequestLogin(ILogger logger, CancellationToken cancellationToken)
    {
        logger.Verbose("Requesting device code...");
        var deviceCodeResponse = await RequestDeviceCode(logger, cancellationToken);
        logger.Information(
            $"Please visit {deviceCodeResponse.VerificationUri} and enter the code: {deviceCodeResponse.UserCode}");

        logger.Verbose("Opening browser...");
        BrowserLauncher.OpenBrowser(deviceCodeResponse.VerificationUriComplete.OriginalString);

        logger.Verbose("Requesting tokens...");
        var tokenResponse = await RequestTokenAsync(deviceCodeResponse, logger, cancellationToken);

        if (tokenResponse != null)
        {
            logger.Verbose("Saving tokens...");
            tokenStore.StoreAccessTokens(new AccessTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken));

            logger.Information("Logged in successfully");
            return;
        }

        logger.Error("Error logging in");
    }

    private static async Task<DeviceAuthorizationResponse> RequestDeviceCode(
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using var client = new RestClient(Authority);
        const string scopes = "openid%20profile%20offline_access";

        var request = new RestRequest("oauth/device/code");
        _ = request.AddHeader("content-type", "application/x-www-form-urlencoded");
        _ = request.AddParameter(
            "application/x-www-form-urlencoded",
            $"client_id={ClientId}&scope={scopes}&audience={Audience}",
            ParameterType.RequestBody);
        var response = await client.PostAsync(request, cancellationToken);

        if (response.IsSuccessful)
        {
            var jsonResponse = JsonSerializer.Deserialize(
                response.Content!,
                JsonContext.Default.DeviceAuthorizationResponse);
            if (jsonResponse is null)
            {
                logger.Error("Error requesting device code: No response content");
                throw new HttpRequestException("No response content");
            }

            return jsonResponse;
        }

        logger.Error($"Error requesting device code: {response.ErrorMessage}");
        throw response.ErrorException ?? throw new HttpRequestException(response.ErrorMessage);
    }


    private static async Task<TokenResponse?> RequestTokenAsync(
        DeviceAuthorizationResponse deviceCodeResponse,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var timeout = TimeSpan.FromSeconds(deviceCodeResponse.ExpiresIn);
        cancellationTokenSource.CancelAfter(timeout);

        logger.Verbose($"Checking if code has been confirmed... (Timeout: {timeout.TotalMinutes} mins)");

        using var client = new RestClient(Authority);

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new RestRequest("oauth/token");
            _ = request.AddHeader("content-type", "application/x-www-form-urlencoded");
            _ = request.AddParameter(
                "application/x-www-form-urlencoded",
                $"grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Adevice_code&device_code={deviceCodeResponse.DeviceCode}&client_id={ClientId}",
                ParameterType.RequestBody);
            var response = await client.ExecutePostAsync(request, cancellationTokenSource.Token);

            if (response.IsSuccessful)
            {
                return JsonSerializer.Deserialize(response.Content!, JsonContext.Default.TokenResponse);
            }

            if (response.Content is not null
                && (response.Content.Contains("authorization_pending", StringComparison.InvariantCulture)
                    || response.Content.Contains("slow_down", StringComparison.InvariantCulture)))
            {
                logger.Verbose($"Code not yet confirmed. Retrying in {deviceCodeResponse.Interval} seconds");
                await Task.Delay(deviceCodeResponse.Interval * 1000, cancellationToken);
            }
            else
            {
                logger.Error($"Error logging in: {response.ErrorMessage}");
                throw response.ErrorException ?? throw new HttpRequestException(response.ErrorMessage);
            }
        }

        logger.Warning($"Requesting tokens timed out after ${timeout} seconds");
        return null;
    }
}
