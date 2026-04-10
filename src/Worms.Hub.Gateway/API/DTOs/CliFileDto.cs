using JetBrains.Annotations;

namespace Worms.Hub.Gateway.API.DTOs;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record CliFileDto(Version LatestVersion, IDictionary<string, string> FileLocations);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record UploadCliFileDto(string Platform, Version Version, IFormFile File);
