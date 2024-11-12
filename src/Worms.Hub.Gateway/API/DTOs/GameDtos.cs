using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

internal sealed record GameDto(string Id, string Status, string HostMachine)
{
    internal static GameDto FromDomain(Game game) => new(game.Id, game.Status, game.HostMachine);

    internal Game ToDomain() => new(Id, Status, HostMachine);
}

internal sealed record CreateGameDto(string HostMachine);
