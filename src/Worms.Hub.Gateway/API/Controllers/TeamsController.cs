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

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] ClaimTeamDto body)
    {
        if (!await featureFlags.IsTeamsEnabledAsync())
        {
            return NotFound();
        }

        var team = teamsRepository.GetById(id);
        if (team is null)
        {
            return NotFound();
        }

        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (body.Claimed)
        {
            if (team.ClaimedByAuth0Subject is not null
                && team.ClaimedByAuth0Subject != callerSubject)
            {
                return Conflict();
            }

            var player = playersRepository.GetByAuth0Subject(callerSubject!);
            if (player is null)
            {
                var displayName = ResolveDisplayName();
                player = playersRepository.Create(new Player(0, callerSubject!, displayName));
            }

            teamsRepository.SetPlayerClaim(id, player.Id);
        }
        else
        {
            if (team.ClaimedByAuth0Subject is not null
                && team.ClaimedByAuth0Subject != callerSubject)
            {
                return Forbid();
            }

            teamsRepository.SetPlayerClaim(id, null);
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
