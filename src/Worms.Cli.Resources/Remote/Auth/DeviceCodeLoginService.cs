using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using Serilog;

namespace Worms.Cli.Resources.Remote.Auth;

public class DeviceCodeLoginService : ILoginService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";
    private const string Audience = "worms.davideadie.dev";
    private readonly ITokenStore _tokenStore;

    public DeviceCodeLoginService(ITokenStore tokenStore) => _tokenStore = tokenStore;

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
            _tokenStore.StoreAccessTokens(new AccessTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken));

            logger.Information("Logged in successfully");
            return;
        }

        logger.Error("Error logging in");
    }

    private static async Task<DeviceAuthorizationResponse> RequestDeviceCode(
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var client = new RestClient(Authority);
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
            return JsonSerializer.Deserialize<DeviceAuthorizationResponse>(response.Content!);
        }

        logger.Error($"Error requesting device code: {response.ErrorMessage}");
        throw response.ErrorException ?? throw new Exception(response.ErrorMessage);
    }

    private record DeviceAuthorizationResponse(
        [property: JsonPropertyName("device_code")]
        string DeviceCode,
        [property: JsonPropertyName("user_code")]
        string UserCode,
        [property: JsonPropertyName("verification_uri")]
        Uri VerificationUri,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: JsonPropertyName("interval")]
        int Interval,
        [property: JsonPropertyName("verification_uri_complete")]
        Uri VerificationUriComplete);

    private static async Task<TokenResponse> RequestTokenAsync(
        DeviceAuthorizationResponse deviceCodeResponse,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var timeout = TimeSpan.FromSeconds(deviceCodeResponse.ExpiresIn);
        cancellationTokenSource.CancelAfter(timeout);

        logger.Verbose($"Checking if code has been confirmed... (Timeout: {timeout.TotalMinutes} mins)");

        var client = new RestClient(Authority);

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
                return JsonSerializer.Deserialize<TokenResponse>(response.Content!);
            }

            if (response.Content is not null
                && (response.Content.Contains("authorization_pending") || response.Content.Contains("slow_down")))
            {
                logger.Verbose($"Code not yet confirmed. Retrying in {deviceCodeResponse.Interval} seconds");
                await Task.Delay(deviceCodeResponse.Interval * 1000, cancellationToken);
            }
            else
            {
                logger.Error($"Error logging in: {response.ErrorMessage}");
                throw response.ErrorException ?? throw new Exception(response.ErrorMessage);
            }
        }

        logger.Warning($"Requesting tokens timed out after ${timeout} seconds");
        return null;
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")]
        string AccessToken,
        [property: JsonPropertyName("refresh_token")]
        string RefreshToken,
        [property: JsonPropertyName("id_token")]
        string IdToken,
        [property: JsonPropertyName("token_type")]
        string TokenType,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn);
}
