using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Worms.Cli;

internal static class LogLevelParser
{
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This is more readable")]
    public static LogLevel GetLogLevel(string[] args)
    {
        var verbose = args.Contains("-v") || args.Contains("--verbose");
        var quiet = args.Contains("-q") || args.Contains("--quiet");

        _ = Activity.Current?.SetTag(Telemetry.Spans.Root.Verbose, verbose);
        _ = Activity.Current?.SetTag(Telemetry.Spans.Root.Quiet, quiet);

        if (verbose)
        {
            return LogLevel.Debug;
        }

        if (quiet)
        {
            return LogLevel.Error;
        }

        return LogLevel.Information;
    }
}
