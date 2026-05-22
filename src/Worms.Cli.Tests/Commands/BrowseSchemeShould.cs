using NUnit.Framework;
using Shouldly;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class BrowseSchemeShould
{
    [Test]
    public async Task OpenTheSchemesFolderWhenWormsIsInstalled()
    {
        using var host = new TestHost();
        var expectedFolder = host.WormsArmageddon.FindInstallation().SchemesFolder;

        var exitCode = await host.Run("browse", "scheme");

        exitCode.ShouldBe(0);
        host.FolderOpener.OpenedFolders.ShouldHaveSingleItem().ShouldBe(expectedFolder);
    }

    [Test]
    public async Task ReturnNonZeroWhenWormsIsNotInstalled()
    {
        using var host = new TestHost(wormsInstalled: false);

        var exitCode = await host.Run("browse", "scheme");

        exitCode.ShouldBe(1);
        host.FolderOpener.OpenedFolders.ShouldBeEmpty();
    }
}
