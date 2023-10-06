namespace Worms.Hub.Gateway.API.Validators;

internal sealed class CliFileValidator
{
    private readonly ILogger<CliFileValidator> _logger;
    private const int MaxFileSize = 1024 * 1024 * 50; // 50MB

    private readonly string[] _fileExtensions =
    {
        ".zip",
        ".tar.gz"
    };

    public CliFileValidator(ILogger<CliFileValidator> logger) => _logger = logger;

    public bool IsValid(IFormFile replayFile, string fileNameForDisplay)
    {
        if (!FileHasCorrectExtension(replayFile))
        {
            _logger.Log(
                LogLevel.Information,
                "Invalid CLI Artifact Uploaded: {Filename} does not have valid extension (.WAGame)",
                fileNameForDisplay);
            return false;
        }

        if (!FileIsExpectedSize(replayFile))
        {
            _logger.Log(
                LogLevel.Information,
                "Invalid CLI Artifact Uploaded: {Filename} is larger than 50MB",
                fileNameForDisplay);
            return false;
        }

        return true;
    }

    private bool FileHasCorrectExtension(IFormFile replayFile) =>
        _fileExtensions.Contains(Path.GetExtension(replayFile.FileName), StringComparer.OrdinalIgnoreCase);

    private static bool FileIsExpectedSize(IFormFile replayFile) => replayFile.Length <= MaxFileSize;
}
