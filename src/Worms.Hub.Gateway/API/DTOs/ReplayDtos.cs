using JetBrains.Annotations;
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
