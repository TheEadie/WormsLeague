using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Tests.Fakes;

internal sealed class RecordingBrowserLauncher : IBrowserLauncher
{
    public List<Uri> OpenedUrls { get; } = [];

    public void OpenBrowser(Uri url) => OpenedUrls.Add(url);
}
