using System.Text.Json.Serialization;

namespace Worms.Gateway.Announcers.Slack
{
    public sealed class SlackMessage
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
