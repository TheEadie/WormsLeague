using System.Threading.Tasks;

namespace Worms.Gateway.Announcers;

public interface ISlackAnnouncer
{
    Task AnnounceGameStarting(string hostName);
}
