using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class GetSchemeShould
{
    [TestCase("scheme")]
    [TestCase("schemes")]
    [TestCase("wsc")]
    public async Task PrintSchemeDetailsForMatchingName(string alias)
    {
        using var host = new TestHost();
        host.WormsArmageddon.WriteScheme("redgate");

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", alias, "redgate");

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("GENERAL");
    }

    [Test]
    public async Task ReturnNonZeroWhenNoSchemeMatchesASpecificName()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("get", "scheme", "nonexistent");

        exitCode.ShouldBe(1);
    }

    [Test]
    public async Task PrintAllMatchingSchemesForWildcardPattern()
    {
        using var host = new TestHost();
        host.WormsArmageddon.WriteScheme("redgate");
        host.WormsArmageddon.WriteScheme("redgate2");

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", "scheme", "*");

        exitCode.ShouldBe(0);
        var output = console.Output.ToString();
        output.ShouldContain("redgate");
        output.ShouldContain("redgate2");
    }

    [Test]
    public async Task ReturnZeroWhenNoSchemesMatchWildcardPattern()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("get", "scheme", "*");

        exitCode.ShouldBe(0);
    }
}
