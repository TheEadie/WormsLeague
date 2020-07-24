using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Worms.DataAccess.Repositories;

namespace Worms.Gateway.Controllers
{
    public class GamesController : V1ApiController
    {
        private readonly IGamesRepo _gamesRepo;

        public GamesController(IGamesRepo gamesRepo)
        {
            _gamesRepo = gamesRepo;
        }

        [HttpGet]
        public ActionResult<IReadOnlyCollection<GameDto>> Get()
        {
            return _gamesRepo.GetAll().Select(x => new GameDto(x.Id, x.Status, x.HostMachine)).ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<GameDto> Get(string id)
        {
            var found = _gamesRepo.Get(id);
            return new GameDto(found.Id, found.Status, found.HostMachine);
        }

        [HttpPost]
        public IActionResult Post(GameDto game)
        {
            _gamesRepo.Add(new Game(game.Id, game.Status, game.HostMachine));
            return CreatedAtAction(nameof(Get), new { id = game.Id }, game);
        }
    }
}
