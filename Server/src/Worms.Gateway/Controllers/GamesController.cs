using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers
{
    public class GamesController : V1ApiController
    {
        private readonly IReadOnlyCollection<GameDto> _gamesRepo;

        public GamesController()
        {
            _gamesRepo = new List<GameDto>
            {
                new GameDto("1", "Pending", "Dev-Something"),
                new GameDto("2", "Complete", "Dev-2"),
            };
        }

        [HttpGet]
        public ActionResult<IReadOnlyCollection<GameDto>> Get()
        {
            return _gamesRepo.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<GameDto> Get(string id)
        {
            var found = _gamesRepo.SingleOrDefault(x => x.Id == id);
            return new GameDto(found.Id, found.Status, found.HostMachine);
        }
    }
}
