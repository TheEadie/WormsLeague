using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class BrowseReplayShould
{
    [Test]
    public async Task OpenTheReplayFolderWhenWormsIsInstalled()
    {
        using var host = new TestHost();
        var expectedFolder = host.Services.GetRequiredService<IWormsArmageddon>().FindInstallation().ReplayFolder;

        var exitCode = await host.Run("browse", "replay");

        exitCode.ShouldBe(0);
        host.FolderOpener.OpenedFolders.ShouldHaveSingleItem()
            .ShouldBe(expectedFolder);
    }

    [Test]
    public async Task ReturnNonZeroWhenWormsIsNotInstalled()
    {
        using var host = new TestHost(wormsInstalled: false);

        var exitCode = await host.Run("browse", "replay");

        exitCode.ShouldBe(1);
        host.FolderOpener.OpenedFolders.ShouldBeEmpty();
    }
}
