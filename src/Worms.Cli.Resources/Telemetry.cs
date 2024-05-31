using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Worms.Cli.Resources;

internal static class Telemetry
{
    private const string SourceName = "Worms.CLI";
    public static readonly ActivitySource Source = new(SourceName);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Using underscores to namespace attributes")]
    internal static class Attributes
    {
        internal static class DownloadLatestCLI
        {
            public const string SpanName = "GET api/v1/files/cli/{platform}";
            public const string Platform = "worms.api.download_latest_cli.platform";
        }

        internal static class GetLatestCliDetails
        {
            public const string SpanName = "GET api/v1/files/cli";
        }
    }
}
