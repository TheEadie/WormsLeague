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
    internal static TeamDto FromDomain(Team team, string? callerAuthSubject) =>
        new(team.Id,
            team.Machine,
            team.TeamName,
            team.ClaimedByPlayerName,
            team.ClaimedByAuthSubject is not null
                && team.ClaimedByAuthSubject == callerAuthSubject);
}

[PublicAPI]
internal sealed record ClaimTeamDto(int Id, bool Claimed);
