using NUnit.Framework;
using Shouldly;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class VersionShould
{
    [Test]
    public async Task PrintCliAndWormsArmageddonVersionsWhenInstalled()
    {
        using var host = new TestHost(wormsInstalled: true);
        using var console = new ConsoleOutputScope();

        var exitCode = await host.Run("version");

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("Worms CLI: 1.0.0");
        console.Output.ToString().ShouldContain("Worms Armageddon: 1.0.0.0");
    }

    [Test]
    public async Task PrintNotInstalledWhenWormsArmageddonMissing()
    {
        using var host = new TestHost(wormsInstalled: false);
        using var console = new ConsoleOutputScope();

        var exitCode = await host.Run("version");

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("Worms Armageddon: Not Installed");
    }
}
