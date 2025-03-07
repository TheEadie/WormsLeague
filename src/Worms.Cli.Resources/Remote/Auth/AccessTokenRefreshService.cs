using System.Diagnostics;
using System.Text.Json;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class AccessTokenRefreshService(IHttpClientFactory httpClientFactory) : IAccessTokenRefreshService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";

    public async Task<AccessTokens> RefreshAccessTokens(AccessTokens current)
    {
        using var span = Activity.Current?.Source.StartActivity(Telemetry.Spans.GetAuthTokens.SpanName);

        if (current.RefreshToken is null)
        {
            throw new InvalidOperationException("Cannot refresh access tokens without a refresh token");
        }

        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(Authority);

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", ClientId },
                { "refresh_token", current.RefreshToken }
            });

        var response = await httpClient.PostAsync(new Uri("oauth/token", UriKind.Relative), content)
            ;

        _ = response.EnsureSuccessStatusCode();

        var streamAsync = await response.Content.ReadAsStreamAsync();
        await using var stream = streamAsync;
        var result =
            await JsonSerializer.DeserializeAsync(streamAsync, JsonContext.Default.TokenResponse)
            ?? throw new JsonException("The API returned success but the JSON response was empty");

        return current with { AccessToken = result.AccessToken };
    }
}
