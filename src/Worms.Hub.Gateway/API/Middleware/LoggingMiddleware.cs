using System.Diagnostics;

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
            EscapeNewLines(request.Method),
            EscapeNewLines(request.Path),
            username);
        _ = Activity.Current?.SetTag("user.id", username);

        await next(context);

        var response = context.Response;
        logger.Log(
            LogLevel.Information,
            "Response: {Method} {Path} by {Username}: {Code}",
            EscapeNewLines(request.Method),
            EscapeNewLines(request.Path),
            username,
            response.StatusCode);
    }

    private static string EscapeNewLines(string input) =>
        input.Replace(Environment.NewLine, "", StringComparison.InvariantCulture);
}

internal static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder) =>
        builder.UseMiddleware<LoggingMiddleware>();
}
