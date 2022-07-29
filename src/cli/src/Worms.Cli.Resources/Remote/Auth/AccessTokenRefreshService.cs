using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Worms.Cli.Resources.Remote.Auth;

internal class AccessTokenRefreshService : IAccessTokenRefreshService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";
    
    public async Task<AccessTokens> RefreshAccessTokens(AccessTokens current)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(Authority);

        var response = await httpClient.PostAsync(new Uri(
                    "oauth/token?" +
                    $"client_id={ClientId}&" +
                    $"refresh_token={current.RefreshToken}&" +
                    "grant_type=refresh_token",
                    UriKind.Relative),
                null)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();
        var streamAsync = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = streamAsync.ConfigureAwait(false);
        var result = await JsonSerializer.DeserializeAsync<TokenResponse>(streamAsync).ConfigureAwait(false);
        if (result is null)
        {
            throw new JsonException("The API returned success but the JSON response was empty");
        }

        return new AccessTokens(result.AccessToken, result.RefreshToken);
    }
    
    private record TokenResponse(
        [property:JsonPropertyName("access_token")] string AccessToken,
        [property:JsonPropertyName("refresh_token")] string RefreshToken,
        [property:JsonPropertyName("id_token")] string IdToken,
        [property:JsonPropertyName("token_type")] string TokenType,
        [property:JsonPropertyName("expires_in")] int ExpiresIn); 
}