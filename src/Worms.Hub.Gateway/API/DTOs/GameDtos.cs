namespace Worms.Hub.Gateway.API.DTOs;

public record GameDto(string Id, string Status, string HostMachine);

public record CreateGameDto(string HostMachine);
