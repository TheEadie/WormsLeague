using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.Ratings;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class TeamsController(
    ITeamsRepository teamsRepository,
    IPlayersRepository playersRepository,
    RatingsCalculator ratingsCalculator) : V1ApiController
{
    [HttpGet]
    public ActionResult<IReadOnlyList<TeamDto>> GetAll()
    {
        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teams = teamsRepository.GetAll();
        return Ok(teams.Select(t => TeamDto.FromDomain(t, callerSubject)).ToList());
    }

    [HttpPut]
    public ActionResult Put(ClaimTeamDto body)
    {
        var team = teamsRepository.GetById(body.Id);
        if (team is null)
        {
            return NotFound();
        }

        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (body.Claimed)
        {
            if (team.IsClaimedByAnother(callerSubject))
            {
                return Conflict();
            }

            var player = playersRepository.GetByAuthSubject(callerSubject!);
            if (player is null)
            {
                var displayName = body.DisplayName ?? ResolveDisplayName();
                player = playersRepository.Create(new Player(callerSubject!, displayName));
            }

            teamsRepository.SetPlayerClaim(body.Id, player.AuthSubject);
        }
        else
        {
            if (team.IsClaimedByAnother(callerSubject))
            {
                return Forbid();
            }

            teamsRepository.SetPlayerClaim(body.Id, null);
        }

        ratingsCalculator.CalculateForTeam(team.Machine, team.TeamName);

        return Ok();
    }

    private string ResolveDisplayName()
    {
        var nickname = User.FindFirstValue("nickname");
        var name = User.FindFirstValue("name");
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return nickname ?? name ?? sub ?? "Unknown";
    }
}
