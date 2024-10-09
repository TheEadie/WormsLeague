using Microsoft.Extensions.Configuration;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Files;

public sealed class CliFiles(IConfiguration configuration)
{
    private const string WindowsFilename = "worms-cli-windows.zip";
    private const string LinuxFilename = "worms-cli-linux.tar.gz";
    private const string VersionFilename = "version.txt";

    public async Task<CliInfo> GetLatestDetails()
    {
        var (latest, platformFilePaths) = GetFilesPaths();
        var versionContent = await File.ReadAllTextAsync(latest).ConfigureAwait(false);

        var version = Version.TryParse(versionContent, out var parsedVersion)
            ? parsedVersion
            : throw new ArgumentException($"Invalid version found in {VersionFilename}");

        var foundPlatformFiles = platformFilePaths.Where(x => File.Exists(x.Value))
            .ToDictionary(x => x.Key, x => Path.GetFileName(x.Value));

        return new CliInfo(version, foundPlatformFiles);
    }

    public Stream GetFileContents(Platform platform)
    {
        var (_, platformFilePaths) = GetFilesPaths();
        var filePath = platformFilePaths[platform];

        return new FileStream(filePath, FileMode.Open);
    }

    public async Task SaveFileContents(Stream fileContentsStream, Platform platform)
    {
        _ = fileContentsStream ?? throw new ArgumentNullException(nameof(fileContentsStream));

        var (_, platformFilePaths) = GetFilesPaths();
        var filePath = platformFilePaths[platform];

        var fileStream = new FileStream(filePath, FileMode.Create);
        await fileContentsStream.CopyToAsync(fileStream).ConfigureAwait(false);
        await fileStream.DisposeAsync().ConfigureAwait(false);
    }

    public async Task SaveLatestVersion(Version version)
    {
        _ = version ?? throw new ArgumentNullException(nameof(version));
        var (filePath, _) = GetFilesPaths();
        await File.WriteAllTextAsync(filePath, version.ToString()).ConfigureAwait(false);
    }

    private (string versionFilePath, IDictionary<Platform, string> platformFilePaths) GetFilesPaths()
    {
        var cliFilesFolder = configuration["Storage:CliFolder"]
            ?? throw new ArgumentException("CLI folder not configured");

        var possiblePlatforms = new Dictionary<Platform, string>
        {
            { Platform.Windows, Path.Combine(cliFilesFolder, WindowsFilename) },
            { Platform.Linux, Path.Combine(cliFilesFolder, LinuxFilename) }
        };

        return (Path.Combine(cliFilesFolder, VersionFilename), possiblePlatforms);
    }
}
