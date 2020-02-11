using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace Worms.Slack
{
    internal class SlackAnnouncer : ISlackAnnouncer
    {
        public async Task AnnounceGameStarting(string hostName, string webHookUrl, ILogger log)
        {
            if (string.IsNullOrWhiteSpace(webHookUrl))
            {
                log.Warning("A Slack web hook must be configured to announce a game");
                return;
            }

            var slackMessage = new SlackMessage
            {
                Text = $"<!here> Hosting at: wa://{hostName}"
            };

            using (HttpClient client = new HttpClient())
            {
                var slackUrl = new System.Uri(webHookUrl);
                var body = JsonConvert.SerializeObject(slackMessage);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(slackUrl, content).ConfigureAwait(false);
                var contents = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}