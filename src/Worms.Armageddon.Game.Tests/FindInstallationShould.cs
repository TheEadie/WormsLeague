using NUnit.Framework;
using Shouldly;

namespace Worms.Armageddon.Game.Tests;

internal sealed class FindInstallationShould
{
    [Test]
    public void NotFindAnInstallationWhenNotInstalled()
    {
        var wormsArmageddon = Fakes.GetWormsArmageddonApi(Fakes.InstallationType.NotInstalled);

        var info = wormsArmageddon.FindInstallation();
        info.ShouldBe(GameInfo.NotInstalled);
    }

    [Test]
    public void FindInstallationFromRegistry()
    {
        var wormsArmageddon = Fakes.GetWormsArmageddonApi(Fakes.InstallationType.Installed);

        var info = wormsArmageddon.FindInstallation();
        info.IsInstalled.ShouldBe(true);
        info.ExeLocation.ShouldBe(@"C:\WormsArmageddon\WA.exe");
        info.ProcessName.ShouldBe("WA");
        info.Version.ShouldBe(new Version(3, 8, 0, 0));
        info.SchemesFolder.ShouldBe(@"C:\WormsArmageddon\User\Schemes");
        info.ReplayFolder.ShouldBe(@"C:\WormsArmageddon\User\Games");
        info.CaptureFolder.ShouldBe(@"C:\WormsArmageddon\User\Capture");
    }
}
