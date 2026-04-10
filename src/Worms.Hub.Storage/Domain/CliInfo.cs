using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record CliInfo(Version Version, IDictionary<Platform, string> PlatformFiles);

[PublicAPI]
public enum Platform
{
    Windows = 0,
    Linux = 1
}
