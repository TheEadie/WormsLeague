using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class LeaguesController(SchemeFiles schemeFiles, LeaguesRepository leaguesRepository) : V1ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeagueDto>>> GetAll()
    {
        var dbLeagues = leaguesRepository.GetAll();
        var tasks = dbLeagues.Select(async dbLeague =>
        {
            var latestDetails = await schemeFiles.GetLatestDetails(dbLeague.Id);
            return LeagueDto.FromDomain(
                dbLeague.Id,
                dbLeague.Name,
                latestDetails,
                new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id = dbLeague.Id })!, UriKind.Relative));
        });
        var results = await Task.WhenAll(tasks);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeagueDto>> Get(string id)
    {
        var dbLeague = leaguesRepository.GetById(id);
        if (dbLeague is null)
        {
            return NotFound();
        }

        var latestDetails = await schemeFiles.GetLatestDetails(id);
        return LeagueDto.FromDomain(
            dbLeague.Id,
            dbLeague.Name,
            latestDetails,
            new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id })!, UriKind.Relative));
    }
}
