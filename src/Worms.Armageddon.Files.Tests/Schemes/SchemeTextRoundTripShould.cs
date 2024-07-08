using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Text;
using Worms.Armageddon.Files.Tests.Schemes.TestData;

namespace Worms.Armageddon.Files.Tests.Schemes;

public sealed class SchemeTextRoundTripShould
{
    private readonly ISchemeTextWriter _writer;
    private readonly ISchemeTextReader _reader;

    public SchemeTextRoundTripShould()
    {
        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonFilesServices();
        var serviceProvider = services.BuildServiceProvider();
        _writer = serviceProvider.GetRequiredService<ISchemeTextWriter>();
        _reader = serviceProvider.GetRequiredService<ISchemeTextReader>();
    }

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
