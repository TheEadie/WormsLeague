using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace Worms.Cli.Slack
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

            var slackMessage = new SlackMessage { Text = $"<!here> Hosting at: wa://{hostName}" };

            using var client = new HttpClient();
            var slackUrl = new System.Uri(webHookUrl);
            var body = JsonSerializer.Serialize(slackMessage);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(slackUrl, content).ConfigureAwait(false);
            await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
