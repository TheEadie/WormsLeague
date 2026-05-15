using JetBrains.Annotations;
using Worms.Armageddon.Files.Replays;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record ReplayDto(
    string Id,
    string Name,
    string Status,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams)
{
    internal static ReplayDto FromDomain(Replay replay) =>
        new(replay.Id, replay.Name, replay.Status, replay.Date, replay.Winner, replay.Teams);
}

[PublicAPI]
internal sealed record CreateReplayDto(string Name, IFormFile ReplayFile);

[PublicAPI]
internal sealed record PlacementDto(string Machine, string TeamName, int Position)
{
    internal static PlacementDto FromDomain(ReplayPlacement p) =>
        new(p.Machine, p.TeamName, p.Position);
}

[PublicAPI]
internal sealed record ReplayDetailDto(
    string Id,
    string Name,
    string Status,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams,
    IReadOnlyList<TurnDto>? Turns,
    IReadOnlyList<PlacementDto>? Placements)
{
    internal static ReplayDetailDto FromDomain(Replay replay, ReplayResource? parsed)
    {
        IReadOnlyList<TurnDto>? turns = null;

        if (parsed is not null)
        {
            var turnList = parsed.Turns.Select(
                (t, i) => new TurnDto(
                    i + 1,
                    t.Team.Name,
                    t.Start.TotalMilliseconds,
                    t.End.TotalMilliseconds,
                    t.Weapons.Select(w => new WeaponDto(w.Name)).ToList(),
                    t.Damage.Select(d => new DamageSummaryDto(d.Team.Name, d.HealthLost, d.WormsKilled)).ToList())).ToList();
            turns = turnList.Count > 0 ? turnList : null;
        }

        var placements = replay.Placements?.Select(PlacementDto.FromDomain).ToList();

        return new ReplayDetailDto(
            replay.Id,
            replay.Name,
            replay.Status,
            replay.Date,
            replay.Winner,
            replay.Teams,
            turns,
            placements);
    }
}

[PublicAPI]
internal sealed record TurnDto(
    int TurnNumber,
    string TeamName,
    double StartMs,
    double EndMs,
    IReadOnlyList<WeaponDto> Weapons,
    IReadOnlyList<DamageSummaryDto> Damage);

[PublicAPI]
internal sealed record WeaponDto(string Name);

[PublicAPI]
internal sealed record DamageSummaryDto(
    string TeamName,
    uint HealthLost,
    uint WormsKilled);
