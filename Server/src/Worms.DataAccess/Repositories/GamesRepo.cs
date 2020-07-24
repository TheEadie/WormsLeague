using System.Collections.Generic;
using System.Linq;
using Worms.DataAccess.Models;

namespace Worms.DataAccess.Repositories
{
    internal class GamesRepo : IGamesRepo
    {
        private readonly DataContext _context;

        public GamesRepo(DataContext context)
        {
            _context = context;
        }

        public IReadOnlyCollection<Game> GetAll()
        {
            return _context.Games.Select(x => x.ToDomain()).ToList();
        }

        public Game Get(string id)
        {
            return _context.Games.Single(x => x.Name == id).ToDomain();
        }

        public void Add(Game game)
        {
            _context.Games.Add(new GameDb()
            {
                Name = game.Id,
                Status = game.Status,
                HostMachine = game.HostMachine
            });

            _context.SaveChanges();
        }
    }
}