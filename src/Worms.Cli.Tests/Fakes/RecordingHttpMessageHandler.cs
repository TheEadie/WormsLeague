using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace Worms.Cli.Tests.Fakes;

internal sealed record RecordedRequest(
    HttpMethod Method,
    Uri? RequestUri,
    AuthenticationHeaderValue? Authorization,
    string? Body);

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
    private readonly List<RecordedRequest> _requests = [];
    private readonly List<(int Count, TaskCompletionSource Tcs)> _waiters = [];
    private readonly Lock _gate = new();
    private Func<HttpRequestMessage, HttpResponseMessage>? _alwaysResponder;

    public IReadOnlyList<RecordedRequest> Requests
    {
        get
        {
            lock (_gate)
            {
                return [.. _requests];
            }
        }
    }

    public Task WaitForRequestCount(int count)
    {
        lock (_gate)
        {
            if (_requests.Count >= count)
            {
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Add((count, tcs));
            return tcs.Task;
        }
    }

    public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        _responses.Enqueue(responder);

    public void EnqueueResponse(HttpStatusCode status, string body, string mediaType = "application/json") =>
        Enqueue(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, mediaType)
        });

    public void EnqueueDeviceCodeResponse(
        string deviceCode = "device-code-123",
        string userCode = "USER-CODE",
        string verificationUri = "https://eadie.eu.auth0.com/activate",
        string verificationUriComplete = "https://eadie.eu.auth0.com/activate?user_code=USER-CODE",
        int expiresInSeconds = 600,
        int intervalSeconds = 5)
    {
        var json =
            $$"""
            {
              "device_code": "{{deviceCode}}",
              "user_code": "{{userCode}}",
              "verification_uri": "{{verificationUri}}",
              "verification_uri_complete": "{{verificationUriComplete}}",
              "expires_in": {{expiresInSeconds.ToString(CultureInfo.InvariantCulture)}},
              "interval": {{intervalSeconds.ToString(CultureInfo.InvariantCulture)}}
            }
            """;
        EnqueueResponse(HttpStatusCode.OK, json);
    }

    public void EnqueueAuthorizationPending() =>
        EnqueueResponse(HttpStatusCode.BadRequest, """{"error":"authorization_pending"}""");

    public void EnqueueSlowDown() =>
        EnqueueResponse(HttpStatusCode.BadRequest, """{"error":"slow_down"}""");

    public void EnqueueSuccessfulTokenResponse(string accessToken, string refreshToken)
    {
        var json =
            $$"""
            {
              "access_token": "{{accessToken}}",
              "refresh_token": "{{refreshToken}}",
              "id_token": "id-token",
              "token_type": "Bearer",
              "expires_in": 3600
            }
            """;
        EnqueueResponse(HttpStatusCode.OK, json);
    }

    public void EnqueueAlways(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        _alwaysResponder = responder;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Test helper captures any reader error so the call site sees the original assertion failure")]
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? body = null;
        if (request.Content is not null)
        {
            try
            {
                body = await request.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception)
            {
                body = null;
            }
        }

        lock (_gate)
        {
            _requests.Add(new RecordedRequest(request.Method, request.RequestUri, request.Headers.Authorization, body));

            for (var i = _waiters.Count - 1; i >= 0; i--)
            {
                if (_requests.Count >= _waiters[i].Count)
                {
                    _waiters[i].Tcs.SetResult();
                    _waiters.RemoveAt(i);
                }
            }
        }

        if (_responses.TryDequeue(out var responder))
        {
            return responder(request);
        }

        if (_alwaysResponder is not null)
        {
            return _alwaysResponder(request);
        }

        throw new InvalidOperationException(
            $"RecordingHttpMessageHandler had no scripted response for {request.Method} {request.RequestUri}");
    }
}
