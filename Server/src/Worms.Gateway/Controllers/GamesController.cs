using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Worms.Gateway.Controllers
{
    public class GamesController : V1ApiController
    {
        [HttpGet]
        public ActionResult<IReadOnlyCollection<GameDto>> Get()
        {
            return new List<GameDto> { new GameDto("1", "InProgress", "localhost") };
        }

        [HttpGet("{id}")]
        public ActionResult<GameDto> Get(string id)
        {
            return new GameDto(id, "InProgress", "localhost");
        }

        [HttpPost]
        public IActionResult Post(GameDto game)
        {
            return CreatedAtAction(nameof(Get), new { id = game.Id }, game);
        }
    }
}
