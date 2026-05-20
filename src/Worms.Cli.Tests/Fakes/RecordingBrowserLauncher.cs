using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Tests.Fakes;

internal sealed class RecordingBrowserLauncher : IBrowserLauncher
{
    public List<string> OpenedUrls { get; } = [];

    public void OpenBrowser(string url) => OpenedUrls.Add(url);
}
