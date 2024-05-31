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
        public const string API_DownloadLatestCLI_Platform = "worms.api.download_latest_cli.platform";
    }
}
