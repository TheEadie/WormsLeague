using System.Text.Json.Serialization;

namespace Worms.Hub.Gateway.Announcers.Slack;

public sealed record SlackMessage([property: JsonPropertyName("text")] string Text);
