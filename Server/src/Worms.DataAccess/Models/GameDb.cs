using Worms.DataAccess.Repositories;

namespace Worms.DataAccess.Models
{
    internal class GameDb
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string HostMachine { get; set; }

        public Game ToDomain()
        {
            return new Game(Name, Status, HostMachine);
        }
    }
}