using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Worms.Gateway.Announcers;
using Worms.Gateway.Database;
using Worms.Gateway.Dtos;

namespace Worms.Gateway.Controllers;

public class GamesController : V1ApiController
{
    private readonly IRepository<GameDto> _repository;
    private readonly ISlackAnnouncer _slackAnnouncer;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IRepository<GameDto> repository,
        ISlackAnnouncer slackAnnouncer,
        ILogger<GamesController> logger)
    {
        _repository = repository;
        _slackAnnouncer = slackAnnouncer;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<GameDto>> Get() => _repository.Get().ToList();

    [HttpGet("{id}")]
    public ActionResult<GameDto> Get(string id)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get games started by {username}", username);

        var found = _repository.Get().SingleOrDefault(x => x.Id == id);

        if (found is null)
        {
            return NotFound($"Game with Id = {id} not found");
        }

        _logger.Log(LogLevel.Information, "Getting games complete");
        return new GameDto(found.Id, found.Status, found.HostMachine);
    }

    [HttpPost]
    public ActionResult<GameDto> Post(CreateGameDto parameters)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Creating game started by {username}", username);

        _slackAnnouncer.AnnounceGameStarting(parameters.HostMachine);

        _logger.Log(LogLevel.Information, "Creating game complete");
        return _repository.Create(new GameDto("0", "Pending", parameters.HostMachine));
    }

    [HttpPut]
    public ActionResult Put(GameDto game)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Updating game started by {username}", username);

        var found = _repository.Get().SingleOrDefault(x => x.Id == game.Id);
        if (found is null)
        {
            return NotFound($"Game with Id = {game.Id} not found");
        }

        _repository.Update(game);

        _logger.Log(LogLevel.Information, "Updating game complete");
        return Ok();
    }
}
