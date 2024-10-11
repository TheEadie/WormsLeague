namespace Worms.Hub.Storage.Domain;

public sealed record CliInfo(Version Version, IDictionary<Platform, string> PlatformFiles);

public enum Platform
{
    Windows = 0,
    Linux = 1
}
