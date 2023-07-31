using System.IO;
using Microsoft.AspNetCore.Http;

namespace Worms.Gateway.Validators;

public class ReplayFileValidator
{
    private const int MaxFileSize = 1024 * 300; // 300KB
    private const string FileExtension = ".wagame";
    private readonly byte[] _fileSignature = "WA"u8.ToArray();

    public bool IsValid(IFormFile replayFile) =>
        FileHasCorrectExtension(replayFile) && FileHasCorrectSignature(replayFile) && FileIsExpectedSize(replayFile);

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
