using System.Text.Json.Serialization;

namespace Worms.Gateway.Domain.Announcers.Slack;

public sealed record SlackMessage([property: JsonPropertyName("text")] string Text);
