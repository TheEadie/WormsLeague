using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Worms.Gateway.Announcers.Slack;

internal class SlackAnnouncer : ISlackAnnouncer
{
    private readonly ILogger<SlackAnnouncer> _logger;
    private readonly string _webHookUrl;

    public SlackAnnouncer(IConfiguration configuration, ILogger<SlackAnnouncer> logger)
    {
        _logger = logger;
        _webHookUrl = configuration.GetValue<string>("SlackWebHookURL");
    }

    public async Task AnnounceGameStarting(string hostName)
    {
        if (string.IsNullOrWhiteSpace(_webHookUrl))
        {
            _logger.LogWarning("A Slack web hook must be configured to announce a game");
            return;
        }

#if DEBUG
        var slackMessage = new SlackMessage
        {
            Text = $"Debug: This is a test run from local dev. Hosting at wa://{hostName}"
        };
#else
            var slackMessage = new SlackMessage { Text = $"<!here> Hosting at: wa://{hostName}" };
#endif

        _logger.LogInformation("Announcing game starting to Slack");
        using var client = new HttpClient();
        var slackUrl = new System.Uri(_webHookUrl);
        var body = JsonSerializer.Serialize(slackMessage);
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(slackUrl, content).ConfigureAwait(false);
        await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}
