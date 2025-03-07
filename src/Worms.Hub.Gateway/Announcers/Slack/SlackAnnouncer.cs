using System.Text;
using System.Text.Json;

namespace Worms.Hub.Gateway.Announcers.Slack;

internal sealed class SlackAnnouncer(IConfiguration configuration, ILogger<SlackAnnouncer> logger) : ISlackAnnouncer
{
    public async Task AnnounceGameStarting(string hostName)
    {
        var webHookUrl = configuration.GetValue<string>("SlackWebHookURL");

        if (string.IsNullOrWhiteSpace(webHookUrl))
        {
            logger.LogWarning("A Slack web hook must be configured to announce a game");
            return;
        }

#if DEBUG
        var slackMessage = new SlackMessage($"Debug: This is a test run from local dev. Hosting at wa://{hostName}");
#else
        var slackMessage = new SlackMessage($"<!here> Hosting at: wa://{hostName}");
#endif

        logger.LogInformation("Announcing game starting to Slack");
        using var client = new HttpClient();
        var slackUrl = new Uri(webHookUrl);
        var body = JsonSerializer.Serialize(slackMessage);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(slackUrl, content);
        await response.Content.ReadAsStringAsync();
    }
}
