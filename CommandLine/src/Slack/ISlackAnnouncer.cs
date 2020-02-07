using System.Threading.Tasks;
using Serilog;

namespace Worms.Slack
{
    internal interface ISlackAnnouncer
    {
        Task AnnounceGameStarting(string hostName, string accessToken, string channelName, ILogger log);
    }
}