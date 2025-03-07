using System.Runtime.Versioning;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[RealDependencies]
[FakeComponent]
internal sealed class FindInstallationShould(ApiType apiType)
{
    [Test]
    public void NotFindAnInstallationWhenNotInstalled()
    {
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, new FakeConfiguration(false));

        var info = wormsArmageddon.FindInstallation();
        info.ShouldBe(GameInfo.NotInstalled);
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public void FindInstallationFromRegistry()
    {
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, new FakeConfiguration(true));

        var info = wormsArmageddon.FindInstallation();
        info.IsInstalled.ShouldBe(true);
        info.ExeLocation.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\WA.exe");
        info.ProcessName.ShouldBe("WA");
        info.Version.ShouldBe(new Version(3, 8, 1, 0));
        info.SchemesFolder.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Schemes");
        info.ReplayFolder.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Games");
        info.CaptureFolder.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Capture");
    }

    [Test]
    [SupportedOSPlatform("linux")]
    public void FindInstallationFromUserHome()
    {
        var linuxUserHome = Environment.GetEnvironmentVariable("HOME");
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, new FakeConfiguration(true));

        var info = wormsArmageddon.FindInstallation();
        info.IsInstalled.ShouldBe(true);
        info.ExeLocation.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/WA.exe");
        info.ProcessName.ShouldBe("WA");
        info.Version.ShouldBe(new Version(3, 8, 1, 0));
        info.SchemesFolder.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/User/Schemes");
        info.ReplayFolder.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/User/Games");
        info.CaptureFolder.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/User/Capture");
    }
}
