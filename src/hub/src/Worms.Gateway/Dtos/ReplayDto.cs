using Microsoft.AspNetCore.Http;
using Worms.Gateway.Domain;

namespace Worms.Gateway.Dtos;

public record ReplayDto(string Id, string Status)
{
    public static ReplayDto FromDomain(Replay replay) => new(replay.Id, replay.Status);
}

public record CreateReplayDto(string Name, IFormFile ReplayFile);
