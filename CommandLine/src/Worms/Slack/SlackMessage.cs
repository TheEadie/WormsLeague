using System.Text.Json.Serialization;

namespace Worms.Slack
{
    public sealed class SlackMessage
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
