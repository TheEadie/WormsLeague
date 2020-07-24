namespace Worms.DataAccess.Repositories
{
    public class Game
    {
        public string Id { get; }
        public string Status { get; }
        public string HostMachine { get; }

        public Game(string id, string status, string hostMachine)
        {
            Id = id;
            Status = status;
            HostMachine = hostMachine;
        }
    }
}