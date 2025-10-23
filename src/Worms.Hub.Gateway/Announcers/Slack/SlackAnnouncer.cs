using System.Text;
using System.Text.Json;

namespace Worms.Hub.Gateway.Announcers.Slack;

internal sealed class Announcer(IConfiguration configuration, ILogger<Announcer> logger) : IAnnouncer
{
    public async Task AnnounceGameStarting(string hostName)
    {
#if DEBUG
        var slackMessage = new SlackMessage($"Debug: This is a test run from local dev. Hosting at wa://{hostName}");
#else
        var slackMessage = new SlackMessage($"<!here> Hosting at: wa://{hostName}");
#endif
        await PostToSlack(slackMessage);
    }

    public async Task AnnounceGameComplete()
    {
#if DEBUG
        var slackMessage = new SlackMessage("Debug: This is a test run from local dev. Game complete!");
#else
        var slackMessage = new SlackMessage($"Game complete!");
#endif
        await PostToSlack(slackMessage);
    }

    private async Task PostToSlack(SlackMessage message)
    {
        var webHookUrl = configuration.GetValue<string>("SlackWebHookURL");

        if (string.IsNullOrWhiteSpace(webHookUrl))
        {
            logger.LogWarning("A Slack web hook must be configured to announce a game");
            return;
        }

        logger.LogInformation("Announcing game starting to Slack");
        using var client = new HttpClient();
        var slackUrl = new Uri(webHookUrl);
        var body = JsonSerializer.Serialize(message);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(slackUrl, content);
        _ = await response.Content.ReadAsStringAsync();
    }
}
