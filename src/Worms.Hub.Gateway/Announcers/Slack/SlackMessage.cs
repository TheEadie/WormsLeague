using System.Text.Json.Serialization;

namespace Worms.Hub.Gateway.Announcers.Slack;

internal sealed record SlackMessage(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("blocks")] string? Blocks = null);
