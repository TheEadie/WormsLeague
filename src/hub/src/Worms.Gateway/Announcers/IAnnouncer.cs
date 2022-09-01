using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Worms.Gateway.Announcers
{
    public interface ISlackAnnouncer
    {
        Task AnnounceGameStarting(string hostName, ILogger log);
    }
}
