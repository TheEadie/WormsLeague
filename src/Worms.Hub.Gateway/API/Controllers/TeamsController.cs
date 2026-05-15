using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.FeatureFlags;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class TeamsController(
    ITeamsRepository teamsRepository,
    IPlayersRepository playersRepository,
    IFeatureFlags featureFlags) : V1ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll()
    {
        if (!await featureFlags.IsTeamsEnabledAsync())
        {
            return NotFound();
        }

        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teams = teamsRepository.GetAll();
        return Ok(teams.Select(t => TeamDto.FromDomain(t, callerSubject)).ToList());
    }

    [HttpPut]
    public async Task<ActionResult> Put(ClaimTeamDto body)
    {
        if (!await featureFlags.IsTeamsEnabledAsync())
        {
            return NotFound();
        }

        var team = teamsRepository.GetById(body.Id);
        if (team is null)
        {
            return NotFound();
        }

        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (body.Claimed)
        {
            if (team.ClaimedByAuthSubject is not null
                && team.ClaimedByAuthSubject != callerSubject)
            {
                return Conflict();
            }

            var player = playersRepository.GetByAuthSubject(callerSubject!);
            if (player is null)
            {
                var displayName = ResolveDisplayName();
                player = playersRepository.Create(new Player(callerSubject!, displayName));
            }

            teamsRepository.SetPlayerClaim(body.Id, player.AuthSubject);
        }
        else
        {
            if (team.ClaimedByAuthSubject is not null
                && team.ClaimedByAuthSubject != callerSubject)
            {
                return Forbid();
            }

            teamsRepository.SetPlayerClaim(body.Id, null);
        }

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
