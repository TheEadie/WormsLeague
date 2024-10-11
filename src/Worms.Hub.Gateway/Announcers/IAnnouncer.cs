namespace Worms.Hub.Gateway.Announcers;

public interface ISlackAnnouncer
{
    Task AnnounceGameStarting(string hostName);
}
