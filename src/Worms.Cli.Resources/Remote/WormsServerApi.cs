using System.IO.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<LatestCliDtoV1> GetLatestCliDetails()
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.GetLatestCliDetails.SpanName);
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/files/cli", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<LatestCliDtoV1>(httpClient, () => httpClient.GetAsync(path))
            .ConfigureAwait(false);
    }

    public async Task<byte[]> DownloadLatestCli(string platform)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.DownloadLatestCLI.SpanName);
        _ = span?.AddTag(Telemetry.Spans.DownloadLatestCLI.Platform, platform);

        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri($"api/v1/files/cli/{platform}", UriKind.Relative);
        return await CallApiBinaryRefreshAccessTokenIfInvalid(httpClient, () => httpClient.GetAsync(path))
            .ConfigureAwait(false);
    }

    public async Task<LeagueDtoV1> GetLeague(string id)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.GetLeague.SpanName);
        _ = span?.AddTag(Telemetry.Spans.League.Id, id);

        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri($"api/v1/leagues/{id}", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<LeagueDtoV1>(httpClient, () => httpClient.GetAsync(path))
            .ConfigureAwait(false);
    }

    public async Task<byte[]> DownloadScheme(string id)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.DownloadScheme.SpanName);
        _ = span?.AddTag(Telemetry.Spans.Scheme.Id, id);

        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri($"api/v1/files/schemes/{id}", UriKind.Relative);
        return await CallApiBinaryRefreshAccessTokenIfInvalid(httpClient, () => httpClient.GetAsync(path))
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<GamesDtoV1>> GetGames()
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/games", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<IReadOnlyCollection<GamesDtoV1>>(
                httpClient,
                () => httpClient.GetAsync(path))
            .ConfigureAwait(false);
    }

    public async Task<GamesDtoV1> CreateGame(CreateGameDtoV1 createParams)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.CreateGame.SpanName);

        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/games", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<GamesDtoV1>(
                httpClient,
                () => httpClient.PostAsJsonAsync(path, createParams, JsonContext.Default.CreateGameDtoV1))
            .ConfigureAwait(false);
    }

    public async Task UpdateGame(GamesDtoV1 newGameDetails)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.UpdateGame.SpanName);
        _ = span?.AddTag(Telemetry.Spans.Game.Id, newGameDetails.Id);
        _ = span?.AddTag(Telemetry.Spans.Game.Status, newGameDetails.Status);

        using var httpClient = _httpClientFactory.CreateClient();
        var path = new Uri("api/v1/games", UriKind.Relative);
        await CallApiRefreshAccessTokenIfInvalid(
                httpClient,
                () => httpClient.PutAsJsonAsync(path, newGameDetails, JsonContext.Default.GamesDtoV1))
            .ConfigureAwait(false);
    }

    public async Task<ReplayDtoV1> CreateReplay(CreateReplayDtoV1 createParams)
    {
        using var span = Telemetry.Source.StartActivity(Telemetry.Spans.CreateReplay.SpanName);
        _ = span?.AddTag(Telemetry.Spans.Replay.Id, createParams.Name);

        using var httpClient = _httpClientFactory.CreateClient();
        using var form = new MultipartFormDataContent();
        var allBytes = await _fileSystem.File.ReadAllBytesAsync(createParams.ReplayFilePath).ConfigureAwait(false);
        using var fileContent = new ByteArrayContent(allBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        using var content = new StringContent(createParams.Name);

        form.Add(content, "Name");
        form.Add(fileContent, "ReplayFile", _fileSystem.Path.GetFileName(createParams.ReplayFilePath));

        var path = new Uri("api/v1/replays", UriKind.Relative);
        return await CallApiRefreshAccessTokenIfInvalid<ReplayDtoV1>(httpClient, () => httpClient.PostAsync(path, form))
            .ConfigureAwait(false);
    }

    private async Task<T> CallApiRefreshAccessTokenIfInvalid<T>(
        HttpClient client,
        Func<Task<HttpResponseMessage>> apiCall)
    {
        var response = await CallApiWithAuthRetry(client, apiCall).ConfigureAwait(false);
        var streamAsync = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = streamAsync.ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(streamAsync, typeof(T), JsonContext.Default)
            .ConfigureAwait(false) is T result
            ? result
            : throw new JsonException("The API returned success but the JSON response was empty");
    }

    private Task CallApiRefreshAccessTokenIfInvalid(HttpClient client, Func<Task<HttpResponseMessage>> apiCall)
    {
        _ = CallApiWithAuthRetry(client, apiCall);
        return Task.CompletedTask;
    }

    private async Task<byte[]> CallApiBinaryRefreshAccessTokenIfInvalid(
        HttpClient client,
        Func<Task<HttpResponseMessage>> apiCall)
    {
        var response = await CallApiWithAuthRetry(client, apiCall).ConfigureAwait(false);
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> CallApiWithAuthRetry(
        HttpClient client,
        Func<Task<HttpResponseMessage>> apiCall)
    {
        client.BaseAddress = _baseUri;
        var accessTokens = _tokenStore.GetAccessTokens();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);

        HttpResponseMessage? response = null;

        const int maxRetries = 3;
        var delayBetweenRetries = 500;
        for (var i = 0; i < maxRetries; i++)
        {
            response = await apiCall().ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Retry with newer access token
                accessTokens = await _accessTokenRefreshService.RefreshAccessTokens(accessTokens).ConfigureAwait(false);
                _tokenStore.StoreAccessTokens(accessTokens);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);
                response = await apiCall().ConfigureAwait(false);
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            if (i != maxRetries - 1)
            {
                await Task.Delay(delayBetweenRetries).ConfigureAwait(false);
            }

            delayBetweenRetries *= 2;
        }

        if (response is null)
        {
            throw new HttpRequestException("The API call failed and no response was returned");
        }

        _ = response.EnsureSuccessStatusCode();
        return response;
    }
}
