using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Middleware;

public class LogUserId(IUserDetailsService userDetailsService, ILogger<LogUserId> logger)
{
    public InvocationMiddleware GetMiddleware() =>
        async (context, next) =>
            {
                if (userDetailsService.IsUserLoggedIn())
                {
                    var anonymisedUserId = userDetailsService.GetAnonymisedUserId();
                    logger.Log(LogLevel.Debug, "Logged in to Worms Hub as {UserId}", anonymisedUserId);
                    _ = Activity.Current?.SetTag("user.id", anonymisedUserId);
                }
                else
                {
                    logger.Log(LogLevel.Debug, "Not logged in to Worms Hub");
                }

                await next(context).ConfigureAwait(false);
            };
}
