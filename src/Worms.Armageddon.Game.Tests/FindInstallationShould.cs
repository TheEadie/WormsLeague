using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game.Tests;

public class FindInstallationShould
{
    private readonly IWormsArmageddon _wormsArmageddon;
    private readonly IRegistry _registry;
    private readonly IFileVersionInfo _fileVersionInfo;

    public FindInstallationShould()
    {
        _registry = Substitute.For<IRegistry>();
        _fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = _fileVersionInfo.GetVersionInfo(Arg.Any<string>()).Returns(new Version(0, 0));

        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonGameServices();
        _ = services.AddScoped<IRegistry>(_ => _registry);
        _ = services.AddScoped<IFileVersionInfo>(_ => _fileVersionInfo);
        var serviceProvider = services.BuildServiceProvider();
        _wormsArmageddon = serviceProvider.GetRequiredService<IWormsArmageddon>();
    }

    [Test]
    public void NotFindAnInstallationWhenNotInstalled()
    {
        var info = _wormsArmageddon.FindInstallation();
        info.ShouldBe(GameInfo.NotInstalled);
    }

    [Test]
    public void FindInstallationFromRegistry()
    {
        _ = _registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns(@"C:\WormsArmageddon");
        _ = _fileVersionInfo.GetVersionInfo(@"C:\WormsArmageddon\WA.exe").Returns(new Version(3, 8, 0, 0));

        var info = _wormsArmageddon.FindInstallation();
        info.IsInstalled.ShouldBe(true);
        info.ExeLocation.ShouldBe(@"C:\WormsArmageddon\WA.exe");
        info.ProcessName.ShouldBe("WA");
        info.Version.ShouldBe(new Version(3, 8, 0, 0));
        info.SchemesFolder.ShouldBe(@"C:\WormsArmageddon\User\Schemes");
        info.ReplayFolder.ShouldBe(@"C:\WormsArmageddon\User\Games");
        info.CaptureFolder.ShouldBe(@"C:\WormsArmageddon\User\Capture");
    }
}
