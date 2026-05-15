using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record TeamDto(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedBy,
    bool IsMyTeam)
{
    internal static TeamDto FromDomain(Team team, string? callerAuth0Subject) =>
        new(team.Id,
            team.Machine,
            team.TeamName,
            team.ClaimedByPlayerName,
            team.ClaimedByAuth0Subject is not null
                && team.ClaimedByAuth0Subject == callerAuth0Subject);
}

[PublicAPI]
internal sealed record ClaimTeamDto(bool Claimed);
