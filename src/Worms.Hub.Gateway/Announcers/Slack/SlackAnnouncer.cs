using System.Text;
using System.Text.Json;

namespace Worms.Hub.Gateway.Announcers.Slack;

internal sealed class Announcer(IConfiguration configuration, ILogger<Announcer> logger) : IAnnouncer
{
    public async Task AnnounceGameStarting(string hostName)
    {
        logger.LogInformation("Announcing game starting to Slack");
        var slackMessage = new SlackMessage($"<!here> Hosting at: wa://{hostName}");
        await PostToSlack(slackMessage);
    }

    public async Task AnnounceGameComplete(string winner)
    {
        logger.LogInformation("Announcing game complete to Slack");
        var slackMessage = new SlackMessage($"Game complete! The winner is: {winner}");
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

#if DEBUG
        var finalMessage = new SlackMessage(
            "Debug:" + message.Text.Replace("<!here>", "", StringComparison.InvariantCulture));
#else
        var finalMessage = message;
#endif

        using var client = new HttpClient();
        var slackUrl = new Uri(webHookUrl);
        var body = JsonSerializer.Serialize(finalMessage);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(slackUrl, content);
        _ = await response.Content.ReadAsStringAsync();
    }
}
