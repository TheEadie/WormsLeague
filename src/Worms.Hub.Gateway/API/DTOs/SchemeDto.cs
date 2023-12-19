using Worms.Hub.Gateway.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

public record SchemeDto(string Id, string Name, Version Version, Uri DownloadUrl)
{
    internal static SchemeDto FromDomain(Scheme scheme, Uri downloadUrl) =>
        new(scheme.Id, scheme.Name, scheme.Version, downloadUrl);
}
