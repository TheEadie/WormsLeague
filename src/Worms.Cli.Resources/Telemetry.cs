using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Worms.Cli.Resources;

internal static class Telemetry
{
    private const string SourceName = "Worms.CLI";
    public static readonly ActivitySource Source = new(SourceName);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Using underscores to namespace attributes")]
    internal static class Spans
    {
        internal static class RequestDeviceCode
        {
            public const string SpanName = "POST oauth/device/code";
        }

        internal static class GetAuthTokens
        {
            public const string SpanName = "POST oauth/token";
        }

        internal static class DownloadLatestCLI
        {
            public const string SpanName = "GET api/v1/files/cli/{platform}";
            public const string Platform = "worms.api.download_latest_cli.platform";
        }

        internal static class GetLatestCliDetails
        {
            public const string SpanName = "GET api/v1/files/cli";
        }

        internal static class GetLeague
        {
            public const string SpanName = "GET api/v1/leagues/{id}";
        }

        internal static class DownloadScheme
        {
            public const string SpanName = "GET api/v1/files/schemes/{id}";
        }

        internal static class CreateGame
        {
            public const string SpanName = "POST api/v1/games";
        }

        internal static class UpdateGame
        {
            public const string SpanName = "PUT api/v1/games/{id}";
        }

        internal static class CreateReplay
        {
            public const string SpanName = "POST api/v1/replays";
        }

        internal static class Game
        {
            public const string Id = "worms.game.id";
            public const string Status = "worms.game.status";
        }

        internal static class Replay
        {
            public const string Id = "worms.replay.id";
        }

        internal static class Scheme
        {
            public const string Id = "worms.scheme.id";
        }

        internal static class League
        {
            public const string Id = "worms.league.id";
        }
    }
}
