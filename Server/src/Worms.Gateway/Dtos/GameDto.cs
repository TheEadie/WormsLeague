namespace Worms.Gateway.Controllers
{
    public class GameDto
    {
        public string Id { get; }
        public string Status { get; }
        public string HostMachine { get; }

        public GameDto(string id, string status, string hostMachine)
        {
            Id = id;
            Status = status;
            HostMachine = hostMachine;
        }
    }
}