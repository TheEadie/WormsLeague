using System.Text.Json.Serialization;

namespace Worms.Gateway.Announcers.Slack;

public sealed record SlackMessage([property: JsonPropertyName("text")] string Text);
