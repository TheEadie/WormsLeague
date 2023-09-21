using System.Text;
using System.Text.Json;
using Serilog;

namespace Worms.Cli.Slack;

internal sealed class SlackAnnouncer : ISlackAnnouncer
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
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(slackUrl, content).ConfigureAwait(false);
        _ = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}