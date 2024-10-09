using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

public record ReplayDto(string Id, string Name, string Status)
{
    internal static ReplayDto FromDomain(Replay replay) => new(replay.Id, replay.Name, replay.Status);
}

public record CreateReplayDto(string Name, IFormFile ReplayFile);
