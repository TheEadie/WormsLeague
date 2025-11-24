using JetBrains.Annotations;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record CliFileDto(Version LatestVersion, IDictionary<string, string> FileLocations);

internal sealed record UploadCliFileDto(string Platform, Version Version, IFormFile File);
