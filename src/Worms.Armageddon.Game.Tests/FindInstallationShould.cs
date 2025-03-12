using System.Runtime.Versioning;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[FakeComponent]
[RealDependencies]
internal sealed class FindInstallationShould(ApiType apiType)
{
    [Test]
    public void NotFindAnInstallationWhenNotInstalled()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).NotInstalled().Build();

        var info = wormsArmageddon.FindInstallation();
        info.ShouldBe(GameInfo.NotInstalled);
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public void FindInstallationFromRegistry()
    {
        const string installationPath = @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\";
        var version = new Version(3, 8, 1, 0);
        var wormsArmageddon = A.WormsArmageddon(apiType).Installed(installationPath, version).Build();

        var info = wormsArmageddon.FindInstallation();
        info.IsInstalled.ShouldBe(true);
        info.ExeLocation.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\WA.exe");
        info.ProcessName.ShouldBe("WA");
        info.Version.ShouldBe(version);
        info.SchemesFolder.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Schemes");
        info.ReplayFolder.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Games");
        info.CaptureFolder.ShouldBe(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Capture");
    }

    [Test]
    [SupportedOSPlatform("linux")]
    public void FindInstallationFromUserHome()
    {
        var linuxUserHome = Environment.GetEnvironmentVariable("HOME");
        var installationPath = $"{linuxUserHome}/.wine/drive_c/WA/";
        var version = new Version(3, 8, 1, 0);
        var wormsArmageddon = A.WormsArmageddon(apiType).Installed(installationPath, version).Build();

        var info = wormsArmageddon.FindInstallation();
        info.IsInstalled.ShouldBe(true);
        info.ExeLocation.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/WA.exe");
        info.ProcessName.ShouldBe("WA");
        info.Version.ShouldBe(version);
        info.SchemesFolder.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/User/Schemes");
        info.ReplayFolder.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/User/Games");
        info.CaptureFolder.ShouldBe($"{linuxUserHome}/.wine/drive_c/WA/User/Capture");
    }
}
