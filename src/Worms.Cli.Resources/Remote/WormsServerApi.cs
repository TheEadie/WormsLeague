using System.IO.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Resources.Remote;

internal sealed class WormsServerApi : IWormsServerApi
{
    private readonly IAccessTokenRefreshService _accessTokenRefreshService;
    private readonly ITokenStore _tokenStore;
    private readonly IFileSystem _fileSystem;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _baseUri;

    public WormsServerApi(
        IAccessTokenRefreshService accessTokenRefreshService,
        ITokenStore tokenStore,
        IFileSystem fileSystem,
        IHttpClientFactory httpClientFactory)
    {
        _accessTokenRefreshService = accessTokenRefreshService;
        _tokenStore = tokenStore;
        _fileSystem = fileSystem;
        _httpClientFactory = httpClientFactory;
#if DEBUG
        _baseUri = new Uri("http://localhost:5005/");
#else
        _baseUri = new Uri("https://worms.davideadie.dev/");
#endif
    }

    public sealed record LatestCliDtoV1(
        [property: JsonPropertyName("latestVersion")]
        Version LatestVersion,
        [property: JsonPropertyName("fileLocations")]
        IReadOnlyDictionary<string, string> FileLocations);

    public async Task<LatestCliDtoV1> GetLatestCliDetails()
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/files/cli", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<LatestCliDtoV1>(
            httpClient,
            async () => await httpClient.GetAsync(path));
    }

    public async Task<byte[]> DownloadLatestCli(string platform)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri($"api/v1/files/cli/{platform}", UriKind.Relative);
        return await CallApiBinaryRefreshAccessTokenIfInvalid(httpClient, async () => await httpClient.GetAsync(path));
    }

    public sealed record GamesDtoV1(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("hostMachine")]
        string HostMachine);

    public sealed record CreateGameDtoV1(
        [property: JsonPropertyName("hostMachine")]
        string HostMachine);

    public async Task<IReadOnlyCollection<GamesDtoV1>> GetGames()
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/games", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<IReadOnlyCollection<GamesDtoV1>>(
            httpClient,
            async () => await httpClient.GetAsync(path));
    }

    public async Task<GamesDtoV1> CreateGame(CreateGameDtoV1 createParams)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/games", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<GamesDtoV1>(
            httpClient,
            async () => await httpClient.PostAsJsonAsync(path, createParams));
    }

    public async Task UpdateGame(GamesDtoV1 newGameDetails)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/games", UriKind.Relative);
        await CallApiRefreshAccessTokenIfInvalid(
            httpClient,
            async () => await httpClient.PutAsJsonAsync(path, newGameDetails));
    }

    public sealed record ReplayDtoV1(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("status")] string Status);

    public sealed record CreateReplayDtoV1(string Name, string ReplayFilePath);

    public async Task<ReplayDtoV1> CreateReplay(CreateReplayDtoV1 createParams)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var form = new MultipartFormDataContent();
        using var fileContent =
            new ByteArrayContent(await _fileSystem.File.ReadAllBytesAsync(createParams.ReplayFilePath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        using var content = new StringContent(createParams.Name);

        form.Add(content, "Name");
        form.Add(fileContent, "ReplayFile", _fileSystem.Path.GetFileName(createParams.ReplayFilePath));

        var path = new Uri("api/v1/replays", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<ReplayDtoV1>(
            httpClient,
            async () => await httpClient.PostAsync(path, form));
    }


    private async Task<T> CallApiRefreshAccessTokenIfInvalid<T>(
        HttpClient client,
        Func<Task<HttpResponseMessage>> apiCall)
    {
        var response = await CallApiWithAuthRetry(client, apiCall);
        var streamAsync = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = streamAsync.ConfigureAwait(false);
        var result = await JsonSerializer.DeserializeAsync<T>(streamAsync).ConfigureAwait(false);
        return result is null
            ? throw new JsonException("The API returned success but the JSON response was empty")
            : result;
    }

    private Task CallApiRefreshAccessTokenIfInvalid(HttpClient client, Func<Task<HttpResponseMessage>> apiCall) =>
        Task.FromResult(_ = CallApiWithAuthRetry(client, apiCall));

    private async Task<byte[]> CallApiBinaryRefreshAccessTokenIfInvalid(
        HttpClient client,
        Func<Task<HttpResponseMessage>> apiCall)
    {
        var response = await CallApiWithAuthRetry(client, apiCall);
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> CallApiWithAuthRetry(
        HttpClient client,
        Func<Task<HttpResponseMessage>> apiCall)
    {
        client.BaseAddress = _baseUri;
        var accessTokens = _tokenStore.GetAccessTokens();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);

        var response = await apiCall().ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Retry with newer access token
            accessTokens = await _accessTokenRefreshService.RefreshAccessTokens(accessTokens).ConfigureAwait(false);
            _tokenStore.StoreAccessTokens(accessTokens);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);
            response = await apiCall().ConfigureAwait(false);
        }

        _ = response.EnsureSuccessStatusCode();
        return response;
    }
}
