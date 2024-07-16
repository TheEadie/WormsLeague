using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Worms.Cli.Logging;

public sealed class ColorLoggerOptions : ConsoleFormatterOptions
{
    public LoggerColorBehavior ColorBehavior { get; set; }
}

internal sealed class ColorFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private ColorLoggerOptions _formatterOptions;

    private bool ConsoleColorFormattingEnabled =>
        _formatterOptions.ColorBehavior == LoggerColorBehavior.Enabled
        || _formatterOptions.ColorBehavior == LoggerColorBehavior.Default && !Console.IsOutputRedirected;

    public ColorFormatter(IOptionsMonitor<ColorLoggerOptions> options)
        : base(nameof(ColorFormatter)) =>
        (_optionsReloadToken, _formatterOptions) = (options.OnChange(ReloadLoggerOptions), options.CurrentValue);

    private void ReloadLoggerOptions(ColorLoggerOptions options) => _formatterOptions = options;

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);

        if (ConsoleColorFormattingEnabled)
        {
            switch (logEntry.LogLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    textWriter.WriteWithColor(message, ConsoleColor.DarkGray);
                    break;
                case LogLevel.Information:
                    textWriter.WriteLine(message);
                    break;
                case LogLevel.Warning:
                    textWriter.WriteWithColor(message, ConsoleColor.Yellow);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    textWriter.WriteWithColor(message, ConsoleColor.Red);
                    if (logEntry.Exception != null)
                    {
                        textWriter.WriteWithColor(logEntry.Exception.Message, ConsoleColor.DarkRed);
                    }

                    break;
                case LogLevel.None:
                    break;
                default:
                    textWriter.WriteLine(message);
                    break;
            }
        }
        else
        {
            textWriter.WriteLine(message);
        }
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}
