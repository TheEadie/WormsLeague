using Microsoft.Extensions.Configuration;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Files;

public sealed class SchemeFiles(IConfiguration configuration)
{
    private const string VersionFilename = "version.txt";

    public async Task<League> GetLatestDetails(string id)
    {
        var (versionPath, schemePath) = GetFilesPaths(id);
        var versionContent = await File.ReadAllTextAsync(versionPath);

        var version = Version.TryParse(versionContent, out var parsedVersion)
            ? parsedVersion
            : throw new ArgumentException($"Invalid version found in {id}-{VersionFilename}");

        return new League(id, id, version, schemePath);
    }

    public Stream GetFileContents(string id)
    {
        var (_, filePath) = GetFilesPaths(id);
        return new FileStream(filePath, FileMode.Open);
    }

    private (string versionFilePath, string schemeFilePath) GetFilesPaths(string name)
    {
        var schemeFilesFolder = configuration["Storage:SchemesFolder"]
            ?? throw new ArgumentException("Scheme folder not configured");

        return (Path.Combine(schemeFilesFolder, $"{name}-{VersionFilename}"),
            Path.Combine(schemeFilesFolder, $"{name}.wsc"));
    }
}
