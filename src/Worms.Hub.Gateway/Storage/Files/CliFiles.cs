using Worms.Hub.Gateway.Domain;

namespace Worms.Hub.Gateway.Storage.Files;

internal sealed class CliFiles(IConfiguration configuration)
{
    private const string WindowsFilename = "worms-cli-windows.zip";
    private const string LinuxFilename = "worms-cli-linux.tar.gz";
    private const string VersionFilename = "version.txt";

    public async Task<CliInfo> GetLatestDetails()
    {
        var (latest, platformFilePaths) = GetFilesPaths();
        var versionContent = await File.ReadAllTextAsync(latest);

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
        var (_, platformFilePaths) = GetFilesPaths();
        var filePath = platformFilePaths[platform];

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await fileContentsStream.CopyToAsync(fileStream);
    }

    public async Task SaveLatestVersion(Version version)
    {
        var (filePath, _) = GetFilesPaths();
        await File.WriteAllTextAsync(filePath, version.ToString());
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
