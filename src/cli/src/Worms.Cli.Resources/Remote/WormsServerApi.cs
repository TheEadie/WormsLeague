using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Resources.Remote;

internal class WormsServerApi : IWormsServerApi
{
    private readonly IAccessTokenRefreshService _accessTokenRefreshService;
    private readonly ITokenStore _tokenStore;
    private readonly HttpClient _httpClient;

    public WormsServerApi(IAccessTokenRefreshService accessTokenRefreshService, ITokenStore tokenStore)
    {
        _accessTokenRefreshService = accessTokenRefreshService;
        _tokenStore = tokenStore;
        _httpClient = new HttpClient();
#if DEBUG
        _httpClient.BaseAddress = new Uri("https://localhost:5001/");
#else
        _httpClient.BaseAddress = new Uri("https://worms.davideadie.dev/");
#endif
    }

    public async Task<IReadOnlyCollection<GamesDtoV1>> GetGames()
    {
        var path = new Uri("api/v1/games", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<IReadOnlyCollection<GamesDtoV1>>(async () =>
            await _httpClient.GetAsync(path));
    }

    public async Task<GamesDtoV1> CreateGame(CreateGameDtoV1 createParams)
    {
        var path = new Uri("api/v1/games", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<GamesDtoV1>(async () =>
            await _httpClient.PostAsJsonAsync(path, createParams));
    }

    public record GamesDtoV1(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("hostMachine")]
        string HostMachine);

    public record CreateGameDtoV1(
        [property: JsonPropertyName("hostMachine")]
        string HostMachine);

    private async Task<T> CallApiRefreshAccessTokenIfInvalid<T>(Func<Task<HttpResponseMessage>> apiCall)
    {
        var accessTokens = _tokenStore.GetAccessTokens();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);

        var response = await apiCall().ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Retry with newer access token
            accessTokens = await _accessTokenRefreshService.RefreshAccessTokens(accessTokens).ConfigureAwait(false);
            _tokenStore.StoreAccessTokens(accessTokens);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);
            response = await apiCall().ConfigureAwait(false);
        }

        _ = response.EnsureSuccessStatusCode();

        var streamAsync = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = streamAsync.ConfigureAwait(false);
        var result = await JsonSerializer.DeserializeAsync<T>(streamAsync).ConfigureAwait(false);
        return result is null
            ? throw new JsonException("The API returned success but the JSON response was empty")
            : (T) result;
    }
}