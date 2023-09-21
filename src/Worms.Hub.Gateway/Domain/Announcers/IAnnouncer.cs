namespace Worms.Hub.Gateway.Domain.Announcers;

public interface ISlackAnnouncer
{
    Task AnnounceGameStarting(string hostName);
}
