using System;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using Serilog.Events;
using Worms.Logging;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    internal abstract class CommandBase
    {
        [Option(ShortName = "v", Description = "Show more information about the process")]
        public bool Verbose { get; }

        [Option(ShortName = "q", Description = "Only show errors")]
        public bool Quiet { get; }

        protected ILogger Logger => _logger.Value;

        private readonly Lazy<ILogger> _logger;

        protected CommandBase()
        {
            _logger = new Lazy<ILogger>(CreateLogger);
        }

        private ILogger CreateLogger()
        {
            var logEventLevel = GetLogEventLevel();
            return new LoggerConfiguration().MinimumLevel.Is(logEventLevel).WriteTo.ColoredConsole().CreateLogger();
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
