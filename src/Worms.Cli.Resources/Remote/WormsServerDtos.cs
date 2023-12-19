using System.Text.Json.Serialization;

namespace Worms.Cli.Resources.Remote;

public sealed record LatestCliDtoV1(
    [property: JsonPropertyName("latestVersion")]
    Version LatestVersion,
    [property: JsonPropertyName("fileLocations")]
    IReadOnlyDictionary<string, string> FileLocations);

public sealed record SchemeDtoV1(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")]
    Version Version,
    [property: JsonPropertyName("downloadUrl")]
    Uri DownloadUrl);

public sealed record CreateGameDtoV1(
    [property: JsonPropertyName("hostMachine")]
    string HostMachine);

public sealed record GamesDtoV1(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("hostMachine")]
    string HostMachine);

public sealed record CreateReplayDtoV1(string Name, string ReplayFilePath);

public sealed record ReplayDtoV1(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status);
