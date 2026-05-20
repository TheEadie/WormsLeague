namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class DefaultBrowserLauncher : IBrowserLauncher
{
    public void OpenBrowser(string url) => BrowserLauncher.OpenBrowser(url);
}
