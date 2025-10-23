namespace Worms.Hub.Gateway.Announcers;

internal interface IAnnouncer
{
    Task AnnounceGameStarting(string hostName);

    Task AnnounceGameComplete(string winner);
}
