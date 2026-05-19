using Microsoft.AspNetCore.Mvc;
using Worms.Armageddon.Files.Replays;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class LeaguesController(
    SchemeFiles schemeFiles,
    ILeaguesRepository leaguesRepository,
    IReplaysRepository replaysRepository,
    IReplayTextReader replayTextReader,
    IRatingsRepository ratingsRepository) : V1ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeagueDto>>> GetAll()
    {
        var dbLeagues = leaguesRepository.GetAll();
        var tasks = dbLeagues.Select(async dbLeague =>
        {
            var latestDetails = await schemeFiles.GetLatestDetails(dbLeague.Id);
            IReadOnlyList<StandingDto> standings = ratingsRepository.GetByLeagueId(dbLeague.Id)
                .Select(r => new StandingDto(r.DisplayName, r.Rating, r.GamesPlayed))
                .ToList();

            return LeagueDto.FromDomain(
                dbLeague.Id,
                dbLeague.Name,
                latestDetails,
                new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id = dbLeague.Id })!, UriKind.Relative),
                standings);
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
        IReadOnlyList<StandingDto> standings = ratingsRepository.GetByLeagueId(id)
            .Select(r => new StandingDto(r.DisplayName, r.Rating, r.GamesPlayed))
            .ToList();

        return LeagueDto.FromDomain(
            dbLeague.Id,
            dbLeague.Name,
            latestDetails,
            new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id })!, UriKind.Relative),
            standings);
    }

    [HttpGet("{id}/replays")]
    public ActionResult<IReadOnlyList<ReplayDetailDto>> GetReplays(string id)
    {
        var league = leaguesRepository.GetById(id);
        if (league is null)
        {
            return NotFound();
        }

        return Ok(replaysRepository.GetByLeagueId(id).Select(replay =>
        {
            ReplayResource? parsed = null;
            if (!string.IsNullOrEmpty(replay.FullLog))
            {
                parsed = replayTextReader.GetModel(replay.FullLog);
            }
            return ReplayDetailDto.FromDomain(replay, parsed);
        }).ToList());
    }

    [HttpGet("{id}/replays/{replayId}")]
    public ActionResult<ReplayDetailDto> GetReplay(string id, string replayId)
    {
        var league = leaguesRepository.GetById(id);
        if (league is null)
        {
            return NotFound();
        }

        var replay = replaysRepository.GetByLeagueId(id).FirstOrDefault(r => r.Id == replayId);
        if (replay is null)
        {
            return NotFound();
        }

        ReplayResource? parsed = null;
        if (!string.IsNullOrEmpty(replay.FullLog))
        {
            parsed = replayTextReader.GetModel(replay.FullLog);
        }

        return ReplayDetailDto.FromDomain(replay, parsed);
    }
}
