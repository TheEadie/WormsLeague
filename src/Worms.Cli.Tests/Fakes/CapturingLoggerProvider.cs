using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Tests.Fakes;

internal sealed record CapturedLogMessage(LogLevel Level, string Category, string Message);

internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentBag<CapturedLogMessage> _messages = [];

    public IReadOnlyCollection<CapturedLogMessage> Messages => [.. _messages];

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _messages);

    public void Dispose()
    {
        // Nothing to dispose.
    }

    private sealed class CapturingLogger(string category, ConcurrentBag<CapturedLogMessage> messages) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            messages.Add(new CapturedLogMessage(logLevel, category, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { /* no-op */ }
        }
    }
}
