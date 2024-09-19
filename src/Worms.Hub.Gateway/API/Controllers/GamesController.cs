using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Domain.Announcers;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class GamesController(
    IRepository<Game> repository,
    ISlackAnnouncer slackAnnouncer,
    ILogger<GamesController> logger) : V1ApiController
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<GameDto>> Get() => repository.GetAll().Select(GameDto.FromDomain).ToList();

    [HttpGet("{id}")]
    public ActionResult<GameDto> Get(string id)
    {
        var found = repository.GetAll().SingleOrDefault(x => x.Id == id);

        if (found is null)
        {
            logger.Log(LogLevel.Information, "Game with Id = {Id} not found", id);
            return NotFound($"Game with Id = {id} not found");
        }

        return GameDto.FromDomain(found);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Post(CreateGameDto parameters)
    {
        await slackAnnouncer.AnnounceGameStarting(parameters.HostMachine).ConfigureAwait(false);
        var game = repository.Create(new Game("0", "Pending", parameters.HostMachine));
        return GameDto.FromDomain(game);
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

        repository.Update(game.ToDomain());
        return Ok();
    }
}
