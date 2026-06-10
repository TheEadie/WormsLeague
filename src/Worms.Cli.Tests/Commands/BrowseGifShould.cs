using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class BrowseGifShould
{
    [TestCase("gif")]
    [TestCase("gifs")]
    public async Task OpenTheCaptureFolderWhenWormsIsInstalled(string alias)
    {
        using var host = new TestHost();
        var expectedFolder = host.WormsArmageddon.FindInstallation().CaptureFolder;

        var exitCode = await host.Run("browse", alias);

        exitCode.ShouldBe(0);
        host.FolderOpener.Received(1).OpenFolder(expectedFolder);
    }

    [Test]
    public async Task ReturnNonZeroWhenWormsIsNotInstalled()
    {
        using var host = new TestHost(wormsInstalled: false);

        var exitCode = await host.Run("browse", "gif");

        exitCode.ShouldBe(1);
        host.FolderOpener.DidNotReceive().OpenFolder(Arg.Any<string>());
    }
}
