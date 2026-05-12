using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record ReplayInLeagueDto(
    string Id,
    string Name,
    bool Processed,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams)
{
    internal static ReplayInLeagueDto FromDomain(Replay replay) =>
        new(
            replay.Id,
            replay.Name,
            replay.Date.HasValue && replay.Winner != null && replay.Teams != null,
            replay.Date,
            replay.Winner,
            replay.Teams);
}
