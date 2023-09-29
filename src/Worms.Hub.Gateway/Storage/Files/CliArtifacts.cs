using Worms.Hub.Gateway.Domain;

namespace Worms.Hub.Gateway.Storage.Files;

internal sealed class CliArtifacts
{
    private readonly IConfiguration _configuration;

    public CliArtifacts(IConfiguration configuration) => _configuration = configuration;

    public async Task<CliInfo> GetLatest()
    {
        var cliArtifactsFolder = _configuration["Storage:CliFolder"]
            ?? throw new ArgumentException("CLI artifact folder not configured");

        var latest = await File.ReadAllTextAsync(Path.Combine(cliArtifactsFolder, "version.txt"));

        var version = Version.TryParse(latest, out var parsedVersion)
            ? parsedVersion
            : throw new ArgumentException("Invalid version found in version.txt");

        var possiblePlatforms = new Dictionary<Platform, string>
        {
            { Platform.Windows, Path.Combine(cliArtifactsFolder, "worms-windows.zip") },
            { Platform.Linux, Path.Combine(cliArtifactsFolder, "worms-linux.tar.gz") }
        };

        var foundPlatformFiles = possiblePlatforms.Where(x => File.Exists(x.Value))
            .ToDictionary(x => x.Key, x => x.Value);

        return new CliInfo(version, foundPlatformFiles);
    }
}
