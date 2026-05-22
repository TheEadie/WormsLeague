using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace Worms.Cli.Tests;

[TestFixture]
internal sealed class LogLevelParserShould
{
    [TestCase("--verbose")]
    [TestCase("-v")]
    public void ReturnDebugForVerbose(string flag) =>
        LogLevelParser.GetLogLevel([flag]).ShouldBe(LogLevel.Debug);

    [TestCase("--quiet")]
    [TestCase("-q")]
    public void ReturnErrorForQuiet(string flag) =>
        LogLevelParser.GetLogLevel([flag]).ShouldBe(LogLevel.Error);

    [Test]
    public void ReturnInformationWhenNoFlags() =>
        LogLevelParser.GetLogLevel([]).ShouldBe(LogLevel.Information);

    [Test]
    public void ReturnDebugWhenBothVerboseAndQuiet() =>
        LogLevelParser.GetLogLevel(["--verbose", "--quiet"]).ShouldBe(LogLevel.Debug);
}
