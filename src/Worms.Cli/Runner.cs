using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Exposed as a composition seam for Worms.Cli.Tests; everything else in the assembly stays internal")]
public sealed class Runner(IUserDetailsService userDetailsService, ILogger<Runner> logger)
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is the global exception handler for the CLI")]
    public async Task<int> Run(Command rootCommand, string[] args, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rootCommand);

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

            var config = new InvocationConfiguration { EnableDefaultExceptionHandler = false };

            return await rootCommand.Parse(args).InvokeAsync(config, cancellationToken);
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
