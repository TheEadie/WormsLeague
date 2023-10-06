using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Domain.Announcers;
using Worms.Hub.Gateway.Storage.Database;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class GamesController : V1ApiController
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
    public ActionResult<IReadOnlyCollection<GameDto>> Get() => _repository.GetAll().ToList();

    [HttpGet("{id}")]
    public ActionResult<GameDto> Get(string id)
    {
        var found = _repository.GetAll().SingleOrDefault(x => x.Id == id);

        if (found is null)
        {
            _logger.Log(LogLevel.Information, "Game with Id = {Id} not found", id);
            return NotFound($"Game with Id = {id} not found");
        }

        return new GameDto(found.Id, found.Status, found.HostMachine);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Post(CreateGameDto parameters)
    {
        await _slackAnnouncer.AnnounceGameStarting(parameters.HostMachine);
        return _repository.Create(new GameDto("0", "Pending", parameters.HostMachine));
    }

    [HttpPut]
    public ActionResult Put(GameDto game)
    {
        var found = _repository.GetAll().SingleOrDefault(x => x.Id == game.Id);
        if (found is null)
        {
            _logger.Log(LogLevel.Information, "Game with Id = {Id} not found", game.Id);
            return NotFound($"Game with Id = {game.Id} not found");
        }

        _repository.Update(game);
        return Ok();
    }
}
