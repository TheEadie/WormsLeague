using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli;

internal sealed class Runner(IUserDetailsService userDetailsService, ILogger<Runner> logger)
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is the global exception handler for the CLI")]
    public async Task<int> Run(CommandLineConfiguration commandLineConfiguration, string[] args)
    {
        try
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

            commandLineConfiguration.EnableDefaultExceptionHandler = false;
            return await commandLineConfiguration.InvokeAsync(args);
        }
        catch (OperationCanceledException)
        {
            return 1;
        }
        catch (Exception exception)
        {
            await Console.Error.WriteAsync("Unhandled exception: ");
            await Console.Error.WriteLineAsync(exception.Message);

            _ = Activity.Current?.AddException(exception);
            _ = Activity.Current?.SetStatus(ActivityStatusCode.Error);

            return 1;
        }
    }
}
