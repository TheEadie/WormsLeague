namespace Worms.Gateway.Dtos;

public record GameDto(string Id, string Status, string HostMachine);

public record CreateGameDto(string HostMachine);
