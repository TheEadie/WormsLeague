using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Domain.Announcers;
using Worms.Hub.Gateway.Storage.Database;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class GamesController(
    IRepository<GameDto> repository,
    ISlackAnnouncer slackAnnouncer,
    ILogger<GamesController> logger) : V1ApiController
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<GameDto>> Get() => repository.GetAll().ToList();

    [HttpGet("{id}")]
    public ActionResult<GameDto> Get(string id)
    {
        var found = repository.GetAll().SingleOrDefault(x => x.Id == id);

        if (found is null)
        {
            logger.Log(LogLevel.Information, "Game with Id = {Id} not found", id);
            return NotFound($"Game with Id = {id} not found");
        }

        return new GameDto(found.Id, found.Status, found.HostMachine);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Post(CreateGameDto parameters)
    {
        await slackAnnouncer.AnnounceGameStarting(parameters.HostMachine);
        return repository.Create(new GameDto("0", "Pending", parameters.HostMachine));
    }

    [HttpPut]
    public ActionResult Put(GameDto game)
    {
        var found = repository.GetAll().SingleOrDefault(x => x.Id == game.Id);
        if (found is null)
        {
            logger.Log(LogLevel.Information, "Game with Id = {Id} not found", game.Id);
            return NotFound($"Game with Id = {game.Id} not found");
        }

        repository.Update(game);
        return Ok();
    }
}
