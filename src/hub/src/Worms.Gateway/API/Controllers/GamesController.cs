using Microsoft.AspNetCore.Mvc;
using Worms.Gateway.API.DTOs;
using Worms.Gateway.Domain.Announcers;
using Worms.Gateway.Storage.Database;

namespace Worms.Gateway.API.Controllers;

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
    public ActionResult<IReadOnlyCollection<GameDto>> Get()
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get games started by {Username}", username);
        var allGames = _repository.GetAll().ToList();
        _logger.Log(LogLevel.Information, "Getting games complete");
        return allGames;
    }

    [HttpGet("{id}")]
    public ActionResult<GameDto> Get(string id)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Get games started by {Username}", username);

        var found = _repository.GetAll().SingleOrDefault(x => x.Id == id);

        if (found is null)
        {
            return NotFound($"Game with Id = {id} not found");
        }

        _logger.Log(LogLevel.Information, "Getting games complete");
        return new GameDto(found.Id, found.Status, found.HostMachine);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Post(CreateGameDto parameters)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Creating game started by {Username}", username);

        await _slackAnnouncer.AnnounceGameStarting(parameters.HostMachine);

        _logger.Log(LogLevel.Information, "Creating game complete");
        return _repository.Create(new GameDto("0", "Pending", parameters.HostMachine));
    }

    [HttpPut]
    public ActionResult Put(GameDto game)
    {
        var username = User.Identity?.Name ?? "anonymous";
        _logger.Log(LogLevel.Information, "Updating game started by {Username}", username);

        var found = _repository.GetAll().SingleOrDefault(x => x.Id == game.Id);
        if (found is null)
        {
            return NotFound($"Game with Id = {game.Id} not found");
        }

        _repository.Update(game);

        _logger.Log(LogLevel.Information, "Updating game complete");
        return Ok();
    }
}
