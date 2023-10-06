namespace Worms.Hub.Gateway.API.Middleware;

internal sealed class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "anonymous";
        var request = context.Request;
        _logger.Log(
            LogLevel.Information,
            "Request: {Method} {Path} by {Username}",
            request.Method,
            request.Path,
            username);

        await _next(context);

        var response = context.Response;
        _logger.Log(
            LogLevel.Information,
            "Response: {Method} {Path} by {Username}: {Code}",
            request.Method,
            request.Path,
            username,
            response.StatusCode);
    }
}

public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder) =>
        builder.UseMiddleware<LoggingMiddleware>();
}
