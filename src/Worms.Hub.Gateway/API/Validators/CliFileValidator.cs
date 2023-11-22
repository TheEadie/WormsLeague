namespace Worms.Hub.Gateway.API.Validators;

internal sealed class CliFileValidator(ILogger<CliFileValidator> logger)
{
    private const int MaxFileSize = 1024 * 1024 * 50; // 50MB

    private readonly string[] _fileExtensions =
    [
        ".zip",
        ".gz"
    ];

    public bool IsValid(IFormFile replayFile, string fileNameForDisplay)
    {
        if (!FileHasCorrectExtension(replayFile))
        {
            logger.Log(
                LogLevel.Information,
                "Invalid CLI Uploaded: {Filename} does not have valid extension (.zip or .tar.gz)",
                fileNameForDisplay);
            return false;
        }

        if (!FileIsExpectedSize(replayFile))
        {
            logger.Log(
                LogLevel.Information,
                "Invalid CLI Uploaded: {Filename} is larger than 50MB",
                fileNameForDisplay);
            return false;
        }

        return true;
    }

    private bool FileHasCorrectExtension(IFormFile replayFile) =>
        _fileExtensions.Contains(Path.GetExtension(replayFile.FileName), StringComparer.OrdinalIgnoreCase);

    private static bool FileIsExpectedSize(IFormFile replayFile) => replayFile.Length <= MaxFileSize;
}
