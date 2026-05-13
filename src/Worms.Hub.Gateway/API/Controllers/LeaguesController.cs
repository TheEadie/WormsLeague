using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.FeatureFlags;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class LeaguesController(
    SchemeFiles schemeFiles,
    LeaguesRepository leaguesRepository,
    ReplaysRepository replaysRepository,
    IFeatureFlags featureFlags) : V1ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeagueDto>>> GetAll()
    {
        if (!await featureFlags.IsLeaguesEnabledAsync())
        {
            return NotFound();
        }

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
        if (!await featureFlags.IsLeaguesEnabledAsync())
        {
            var filesystemDetails = await schemeFiles.GetLatestDetails(id);
            if (filesystemDetails is null)
            {
                return NotFound();
            }
            return LeagueDto.FromDomain(
                filesystemDetails.Id,
                filesystemDetails.Name,
                filesystemDetails,
                new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id })!, UriKind.Relative));
        }

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

    [HttpGet("{id}/replays")]
    public async Task<ActionResult<IReadOnlyList<ReplayDto>>> GetReplays(string id)
    {
        if (!await featureFlags.IsReplayLeagueFieldsEnabledAsync())
        {
            return NotFound();
        }

        var league = leaguesRepository.GetById(id);
        if (league is null)
        {
            return NotFound();
        }

        var replays = replaysRepository.GetByLeagueId(id);
        return Ok(replays.Select(ReplayDto.FromDomain).ToList());
    }
}
