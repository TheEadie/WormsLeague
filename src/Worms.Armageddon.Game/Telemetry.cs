namespace Worms.Armageddon.Game;

public static class Telemetry
{
    internal static class Spans
    {
        internal static class WormsArmageddon
        {
            public const string SpanName = "Worms Armageddon";
            public const string Version = "worms.armageddon.version";
            public const string Args = "worms.armageddon.args";
            public const string ExitCode = "worms.armageddon.exit_code";
        }

        internal static class ProcessRunner
        {
            public const string TimeToFindProcess = "worms.process.runner.time_to_find_process_ms";
        }
    }
}
