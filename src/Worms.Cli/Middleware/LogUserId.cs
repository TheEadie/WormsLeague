using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Middleware;

internal sealed class LogUserId(IUserDetailsService userDetailsService, ILogger<LogUserId> logger)
{
    public InvocationMiddleware GetMiddleware() =>
        async (context, next) =>
            {
                if (userDetailsService.IsUserLoggedIn())
                {
                    var anonymisedUserId = userDetailsService.GetAnonymisedUserId();
                    logger.Log(LogLevel.Debug, "Logged in to Worms Hub as {UserId}", anonymisedUserId);
                    _ = Activity.Current?.SetTag(Telemetry.Spans.Root.LoggedIn, true);
                    _ = Activity.Current?.SetTag(Telemetry.Spans.Root.UserId, anonymisedUserId);
                }
                else
                {
                    logger.Log(LogLevel.Debug, "Not logged in to Worms Hub");
                    _ = Activity.Current?.SetTag(Telemetry.Spans.Root.LoggedIn, false);
                }

                await next(context);
            };
}
