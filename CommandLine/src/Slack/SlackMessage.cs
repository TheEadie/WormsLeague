using Newtonsoft.Json;

namespace Worms.Slack
{
    public sealed class SlackMessage
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}