using System.Text.Json.Serialization;

namespace Worms.Cli.Slack;

public sealed class SlackMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
