namespace Worms.Hub.Gateway.API.Middleware;

internal sealed class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "anonymous";
        var request = context.Request;
        logger.Log(
            LogLevel.Information,
            "Request: {Method} {Path} by {Username}",
            request.Method,
            request.Path,
            username);

        await next(context).ConfigureAwait(false);

        var response = context.Response;
        logger.Log(
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
