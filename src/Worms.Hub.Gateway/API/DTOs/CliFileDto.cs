namespace Worms.Hub.Gateway.API.DTOs;

public record CliFileDto(Version LatestVersion, IDictionary<string, string> FileLocations);

public record UploadCliFileDto(string Platform, Version Version, IFormFile File);
