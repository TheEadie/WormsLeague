namespace Worms.Hub.Gateway.Announcers;

internal interface ISlackAnnouncer
{
    Task AnnounceGameStarting(string hostName);
}
