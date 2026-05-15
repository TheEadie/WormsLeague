using System.Text;
using System.Text.Json;

namespace Worms.Hub.Gateway.Announcers.Slack;

internal sealed class Announcer(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<Announcer> logger) : IAnnouncer
{
    public async Task AnnounceGameStarting(string hostName)
    {
        logger.LogInformation("Announcing game starting to Slack");
        var slackMessage = new SlackMessage($"<!here> Hosting at: wa://{hostName}");
        await PostToSlack(slackMessage);
    }

    public async Task AnnounceGameComplete(string winner, IReadOnlyList<PlacementInfo>? placements = null)
    {
        logger.LogInformation("Announcing game complete to Slack");

        string headerText;
        string bodyText;
        if (placements?.Count > 0)
        {
            headerText = "Results:";
            bodyText = string.Join("\n", placements
                .OrderBy(p => p.Position)
                .Select(p => $"{p.Position}: {p.TeamName}"));
        }
        else
        {
            headerText = "Winner:";
            bodyText = winner;
        }

        var slackMessage = new SlackMessage(
            "Game Complete",
            $$"""
              [
                  {
                      "type": "header",
                      "text": {
                          "type": "plain_text",
                          "text": "Mayhem complete",
                          "emoji": true
                      }
                  },
                  {
                      "type": "section",
                      "text": {
                          "type": "mrkdwn",
                          "text": "*{{headerText}}*\n{{bodyText}}"
                      }
                  }
              ]
              """);
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
            Text: "Debug: " + message.Text.Replace("<!here>", "", StringComparison.InvariantCulture),
            Blocks: message.Blocks?.Replace("<!here>", "", StringComparison.InvariantCulture));
#else
        var finalMessage = message;
#endif

        using var client = httpClientFactory.CreateClient();
        var slackUrl = new Uri(webHookUrl);
        var body = JsonSerializer.Serialize(finalMessage);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(slackUrl, content);
        _ = await response.Content.ReadAsStringAsync();
    }
}
