using Serilog;

namespace Worms.Cli.Slack
{
    internal interface ISlackAnnouncer
    {
        Task AnnounceGameStarting(string hostName, string webHookUrl, ILogger log);
    }
}
