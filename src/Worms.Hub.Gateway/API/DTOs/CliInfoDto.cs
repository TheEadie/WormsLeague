namespace Worms.Hub.Gateway.API.DTOs;

public record CliInfoDto(Version Version, IDictionary<string, string> Downloads);

public record UploadCliDto(string Platform, IFormFile CliFile);
