using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

public record GameDto(string Id, string Status, string HostMachine)
{
    internal static GameDto FromDomain(Game game) => new(game.Id, game.Status, game.HostMachine);

    internal Game ToDomain() => new(Id, Status, HostMachine);
}

public record CreateGameDto(string HostMachine);
