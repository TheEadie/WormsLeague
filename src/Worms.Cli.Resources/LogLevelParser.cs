using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Resources;

public static class LogLevelParser
{
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This is more readable")]
    public static LogLevel GetLogLevel(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var verbose = args.Contains("-v") || args.Contains("--verbose");
        var quiet = args.Contains("-q") || args.Contains("--quiet");

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
