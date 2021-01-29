using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Serilog;

namespace Worms.Server.Auth
{
    public class DeviceCodeLoginService : ILoginService
    {
        private const string Authority = "https://eadie.eu.auth0.com";
        private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";
        private const string Audience = "worms.davideadie.dev";
        private readonly ITokenStore _tokenStore;

        public DeviceCodeLoginService(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        public async Task RequestLogin(ILogger logger, CancellationToken cancellationToken)
        {
            logger.Verbose("Requesting device code...");
            var deviceCodeResponse = await RequestDeviceCode(logger, cancellationToken);
            logger.Information($"Please visit {deviceCodeResponse.VerificationUri} and enter the code: {deviceCodeResponse.UserCode}");

            logger.Verbose("Opening browser...");
            SystemBrowser.OpenBrowser(deviceCodeResponse.VerificationUriComplete);

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
            var httpClient = new HttpClient();
            var response = await httpClient.RequestDeviceAuthorizationAsync(
                new DeviceAuthorizationRequest
                {
                    Address = $"{Authority}/oauth/device/code",
                    ClientId = ClientId,
                    Scope = "openid profile offline_access",
                    Parameters =
                    {
                        {"audience", Audience},
                    }
                },
                cancellationToken);

            if (!response.IsError) return response;

            logger.Error($"Error requesting device code: {response.ErrorDescription}");
            throw new Exception(response.Error);

        }

        private static async Task<TokenResponse> RequestTokenAsync(
            DeviceAuthorizationResponse deviceCodeResponse,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            var timeout = TimeSpan.FromSeconds(Convert.ToDouble(deviceCodeResponse.ExpiresIn));
            cancellationTokenSource.CancelAfter(timeout);

            logger.Verbose($"Checking if code has been confirmed... (Timeout: {timeout.TotalMinutes} mins)");

            var httpClient = new HttpClient();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await httpClient.RequestDeviceTokenAsync(
                    new DeviceTokenRequest
                    {
                        Address = $"{Authority}/oauth/token",
                        ClientId = ClientId,
                        DeviceCode = deviceCodeResponse.DeviceCode
                    },
                    cancellationToken);

                if (!response.IsError) return response;

                if (response.Raw.Contains("authorization_pending") || response.Raw.Contains("slow_down"))
                {
                    logger.Verbose($"Code not yet confirmed. Retrying in {deviceCodeResponse.Interval} seconds");
                    await Task.Delay(deviceCodeResponse.Interval * 1000, cancellationToken);
                }
                else
                {
                    logger.Error($"Error logging in: {response.ErrorDescription}");
                    throw new Exception(response.Error);
                }
            }

            logger.Warning($"Requesting tokens timed out after ${timeout} seconds");
            return null;
        }
    }
}
