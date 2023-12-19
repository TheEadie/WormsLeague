using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files.Tests.Schemes;

public sealed class SchemeTextRoundTripShould
{
    private readonly SchemeTextWriter _writer = new();
    private readonly SchemeTextReader _reader = new();

    [Test]
    [TestCaseSource(nameof(Schemes))]
    public void NotLoseInformation(Scheme input)
    {
        var result = RoundTrip(input);
        result.ShouldBe(input);
    }

    private Scheme RoundTrip(Scheme scheme)
    {
        using var output = new StringWriter();
        _writer.Write(scheme, output);
        var input = output.ToString();
        return _reader.GetModel(input);
    }

    private static IEnumerable<TestCaseData> Schemes => TestSchemes.Schemes();
}
