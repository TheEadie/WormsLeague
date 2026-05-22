using NUnit.Framework;
using Shouldly;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class DeleteSchemeShould
{
    [Test]
    public async Task RemoveTheSchemeFile()
    {
        using var host = new TestHost();
        var folder = host.WormsArmageddon.FindInstallation().SchemesFolder;
        SchemeFixtures.WriteScheme(host, "redgate");

        var exitCode = await host.Run("delete", "scheme", "redgate");

        exitCode.ShouldBe(0);
        host.FileSystem.File.Exists(Path.Combine(folder, "redgate.wsc")).ShouldBeFalse();
    }

    [Test]
    public async Task ReturnNonZeroWhenNoSchemeMatches()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("delete", "scheme", "missing");

        exitCode.ShouldBe(1);
    }

    [Test]
    public async Task ReturnNonZeroAndDeleteNothingWhenMultipleSchemesMatch()
    {
        using var host = new TestHost();
        var folder = host.WormsArmageddon.FindInstallation().SchemesFolder;
        SchemeFixtures.WriteScheme(host, "redgate");
        SchemeFixtures.WriteScheme(host, "redgate2");

        var exitCode = await host.Run("delete", "scheme", "*");

        exitCode.ShouldBe(1);
        host.FileSystem.File.Exists(Path.Combine(folder, "redgate.wsc")).ShouldBeTrue();
        host.FileSystem.File.Exists(Path.Combine(folder, "redgate2.wsc")).ShouldBeTrue();
    }
}
