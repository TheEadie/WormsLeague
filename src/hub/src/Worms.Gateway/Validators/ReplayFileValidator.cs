﻿namespace Worms.Gateway.Validators;

public class ReplayFileValidator
{
    private readonly ILogger<ReplayFileValidator> _logger;
    private const int MaxFileSize = 1024 * 300; // 300KB
    private const string FileExtension = ".wagame";
    private readonly byte[] _fileSignature = "WA"u8.ToArray();

    public ReplayFileValidator(ILogger<ReplayFileValidator> logger)
    {
        _logger = logger;
    }

    public bool IsValid(IFormFile replayFile, string fileNameForDisplay)
    {
        if (!FileHasCorrectExtension(replayFile))
        {
            _logger.Log(
                LogLevel.Information,
                "Invalid Replay Uploaded: {filename} does not have valid extension (.WAGame)",
                fileNameForDisplay);
            return false;
        }

        if (!FileHasCorrectSignature(replayFile))
        {
            _logger.Log(
                LogLevel.Information,
                "Invalid Replay Uploaded: {filename} does not have valid signature (WA)",
                fileNameForDisplay);
            return false;
        }

        if (!FileIsExpectedSize(replayFile))
        {
            _logger.Log(
                LogLevel.Information,
                "Invalid Replay Uploaded: {filename} is larger than 300KB",
                fileNameForDisplay);
            return false;
        }

        return true;
    }

    private static bool FileHasCorrectExtension(IFormFile replayFile) =>
        Path.GetExtension(replayFile.FileName).ToLowerInvariant() == FileExtension;

    private bool FileHasCorrectSignature(IFormFile replayFile)
    {
        using var stream = replayFile.OpenReadStream();
        var buffer = new byte[2];
        var bytesRead = stream.Read(buffer);

        if (bytesRead < 2)
        {
            return false;
        }

        return buffer[0] == _fileSignature[0] && buffer[1] == _fileSignature[1];
    }

    private static bool FileIsExpectedSize(IFormFile replayFile) => replayFile.Length <= MaxFileSize;
}
