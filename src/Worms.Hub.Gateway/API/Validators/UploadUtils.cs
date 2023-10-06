using System.Net;

namespace Worms.Hub.Gateway.API.Validators;

internal static class UploadUtils
{
    public static string GetFileNameForDisplay(IFormFile replayFile)
    {
        var untrustedFileName = Path.GetFileName(replayFile.FileName);
        return WebUtility.HtmlEncode(untrustedFileName);
    }
}
