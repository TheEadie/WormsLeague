using System.Text.Json;
using RestSharp;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class AccessTokenRefreshService : IAccessTokenRefreshService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";

    public async Task<AccessTokens> RefreshAccessTokens(AccessTokens current)
    {
        using var client = new RestClient(Authority);
        var request = new RestRequest("oauth/token", Method.Post);
        _ = request.AddHeader("content-type", "application/x-www-form-urlencoded");
        _ = request.AddParameter(
            "application/x-www-form-urlencoded",
            $"grant_type=refresh_token&client_id={ClientId}&refresh_token={current.RefreshToken}",
            ParameterType.RequestBody);
        var response = await client.ExecuteAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessful)
        {
            throw response.ErrorException ?? throw new HttpRequestException(response.ErrorMessage);
        }

        var result = JsonSerializer.Deserialize(response.Content!, JsonContext.Default.TokenResponse)
            ?? throw new JsonException("The API returned success but the JSON response was empty");

        return current with { AccessToken = result.AccessToken };
    }
}
