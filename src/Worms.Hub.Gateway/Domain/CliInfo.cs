namespace Worms.Hub.Gateway.Domain;

internal sealed record CliInfo(Version Version, IDictionary<Platform, string> PlatformFiles);

internal enum Platform
{
    Windows = 0,
    Linux = 1
}
