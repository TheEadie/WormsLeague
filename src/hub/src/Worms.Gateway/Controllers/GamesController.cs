using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Worms.Gateway.Database;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers
{
    public class GamesController : V1ApiController
    {
        private readonly IRepository<GameDto> _repository;

        public GamesController(IRepository<GameDto> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IReadOnlyCollection<GameDto>> Get()
        {
            return _repository.Get().ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<GameDto> Get(string id)
        {
            var found = _repository.Get().SingleOrDefault(x => x.Id == id);
            return new GameDto(found.Id, found.Status, found.HostMachine);
        }
        
        [HttpPost]
        public ActionResult<GameDto> Post(CreateGameDto parameters)
        {
            return _repository.Create(new GameDto("0", "Pending", parameters.HostMachine));
        }
    }
}
