using System.Text.Json;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class AccessTokenRefreshService(IHttpClientFactory httpClientFactory) : IAccessTokenRefreshService
{
    private const string Authority = "https://eadie.eu.auth0.com";
    private const string ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB";

    public async Task<AccessTokens> RefreshAccessTokens(AccessTokens current)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.GetAuthTokens.SpanName);

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
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var streamAsync = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = streamAsync.ConfigureAwait(false);
        var result =
            await JsonSerializer.DeserializeAsync(streamAsync, JsonContext.Default.TokenResponse).ConfigureAwait(false)
            ?? throw new JsonException("The API returned success but the JSON response was empty");

        return current with { AccessToken = result.AccessToken };
    }
}
