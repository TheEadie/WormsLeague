using Newtonsoft.Json;

namespace Worms.Slack
{
    public sealed class SlackMessage
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("as_user")]
        public bool UserName { get => true; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}