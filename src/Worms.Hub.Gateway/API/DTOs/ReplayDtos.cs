using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ReplayDto(string Id, string Name, string Status)
{
    internal static ReplayDto FromDomain(Replay replay) => new(replay.Id, replay.Name, replay.Status);
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CreateReplayDto(string Name, IFormFile ReplayFile);
