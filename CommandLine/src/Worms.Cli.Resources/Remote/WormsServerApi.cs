using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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
        _httpClient.BaseAddress = new Uri("https://worms.davideadie.dev");
    }
    
    public async Task<T> Get<T>(Uri path)
    {
        var accessTokens = _tokenStore.GetAccessTokens();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);

        var response = await _httpClient.GetAsync(path).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Retry with newer access token
            accessTokens = await _accessTokenRefreshService.RefreshAccessTokens(accessTokens).ConfigureAwait(false);
            _tokenStore.StoreAccessTokens(accessTokens);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);
            response = await _httpClient.GetAsync(path).ConfigureAwait(false);
        }

        _ = response.EnsureSuccessStatusCode();

        var streamAsync = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = streamAsync.ConfigureAwait(false);
        var result = await JsonSerializer.DeserializeAsync<T>(streamAsync).ConfigureAwait(false);
        return result is null ? throw new JsonException("The API returned success but the JSON response was empty") : (T) result;
    }
}