using System.Diagnostics;

namespace Worms.Armageddon.Game;

public static class Telemetry
{
    private const string SourceName = "Worms.CLI";
    public static readonly ActivitySource Source = new(SourceName);

    internal static class Spans
    {
        internal static class WormsArmageddon
        {
            public const string SpanName = "Worms Armageddon";
            public const string Version = "worms.armageddon.version";
            public const string Args = "worms.armageddon.args";
        }
    }
}
