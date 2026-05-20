using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Worms.Cli.Resources.Remote.Auth;

public sealed class BrowserLauncher : IBrowserLauncher
{
    public void OpenBrowser(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var target = url.OriginalString;
        try
        {
            _ = Process.Start(target);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                target = target.Replace("&", "^&", StringComparison.InvariantCulture);
                _ = Process.Start(new ProcessStartInfo("cmd", $"/c start {target}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _ = Process.Start("xdg-open", target);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _ = Process.Start("open", target);
            }
            else
            {
                throw;
            }
        }
    }
}
