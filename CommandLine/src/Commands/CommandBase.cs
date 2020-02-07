using System;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Worms.Logging;

namespace Worms.Commands
{
    internal abstract class CommandBase
    {
        [Option(ShortName = "v", Description = "Show more information about the process")]
        public bool Verbose { get; set; }

        [Option(ShortName = "q", Description = "Only show errors")]
        public bool Quiet { get; set; }

        protected ILogger Logger => _logger.Value;

        private readonly Lazy<ILogger> _logger;

        protected CommandBase()
        {
            _logger = new Lazy<ILogger>(CreateLogger);
        }

        private ILogger CreateLogger()
        {
            var logEventLevel = GetLogEventLevel();
            return new LoggerConfiguration()
                .MinimumLevel.Is(logEventLevel)
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }

        private LogEventLevel GetLogEventLevel()
        {
            if (Verbose)
            {
                return LogEventLevel.Verbose;
            }

            if (Quiet)
            {
                return LogEventLevel.Error;
            }

            return LogEventLevel.Information;
        }
    }
}
