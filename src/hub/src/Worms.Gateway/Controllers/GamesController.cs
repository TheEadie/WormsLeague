using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Worms.Gateway.Announcers;
using Worms.Gateway.Database;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers
{
    public class GamesController : V1ApiController
    {
        private readonly IRepository<GameDto> _repository;
        private readonly ISlackAnnouncer _slackAnnouncer;
        private readonly ILogger _logger;

        public GamesController(IRepository<GameDto> repository, ISlackAnnouncer slackAnnouncer, ILogger logger)
        {
            _repository = repository;
            _slackAnnouncer = slackAnnouncer;
            _logger = logger;
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
            _slackAnnouncer.AnnounceGameStarting(parameters.HostMachine, _logger);
            return _repository.Create(new GameDto("0", "Pending", parameters.HostMachine));
        }
    }
}
