namespace Worms.Hub.Gateway.API.DTOs;

public record CliInfoDto(Version Version, IDictionary<string, string> Downloads);
