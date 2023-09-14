using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Worms.Cli.Resources.Remote.Auth;

internal static class BrowserLauncher
{
    public static void OpenBrowser(string url)
    {
        try
        {
            _ = Process.Start(url);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                _ = Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _ = Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _ = Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
