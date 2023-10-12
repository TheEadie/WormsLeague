using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Worms.Cli.Logging;

internal sealed class ColoredConsoleSink : ILogEventSink
{
    private readonly ConsoleColor _defaultForeground = Console.ForegroundColor;
    private readonly ConsoleColor _defaultBackground = Console.BackgroundColor;

    private readonly ITextFormatter _formatter;

    public ColoredConsoleSink(ITextFormatter formatter) => _formatter = formatter;

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level >= LogEventLevel.Fatal)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Red;
        }
        else if (logEvent.Level >= LogEventLevel.Error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (logEvent.Level >= LogEventLevel.Warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else if (logEvent.Level >= LogEventLevel.Information)
        {
            Console.ForegroundColor = _defaultForeground;
        }
        else if (logEvent.Level >= LogEventLevel.Verbose)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        _formatter.Format(logEvent, Console.Out);
        Console.Out.Flush();

        Console.ForegroundColor = _defaultForeground;
        Console.BackgroundColor = _defaultBackground;
    }
}

internal static class ColoredConsoleSinkExtensions
{
    public static LoggerConfiguration ColoredConsole(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider formatProvider,
        LogEventLevel minimumLevel = LogEventLevel.Verbose,
        string outputTemplate = "{Message:lj}{NewLine}{Exception}") =>
        loggerConfiguration.Sink(
            new ColoredConsoleSink(new MessageTemplateTextFormatter(outputTemplate, formatProvider)),
            minimumLevel);
}
