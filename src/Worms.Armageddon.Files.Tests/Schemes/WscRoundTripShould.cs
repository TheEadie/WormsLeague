using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Binary;

namespace Worms.Armageddon.Files.Tests.Schemes;

public sealed class WscRoundTripShould : IDisposable
{
    private readonly IWscWriter _writer;
    private readonly IWscReader _reader;
    private readonly string _tempDirectory;
    private readonly string _file;

    public WscRoundTripShould()
    {
        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonFilesServices();
        var serviceProvider = services.BuildServiceProvider();
        _writer = serviceProvider.GetRequiredService<IWscWriter>();
        _reader = serviceProvider.GetRequiredService<IWscReader>();

        _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _file = Path.Combine(_tempDirectory, Path.GetRandomFileName() + ".wsc");

        _ = Directory.CreateDirectory(_tempDirectory);
    }

    [Test]
    [TestCaseSource(nameof(Schemes))]
    public void NotLoseInformation(Scheme input)
    {
        _writer.Write(input, _file);
        var result = _reader.Read(_file);
        result.ShouldBe(input);
    }

    [Test]
    // This setting has the wrong default value in the underlying library
    public void SetFiringPausesTimerToTrueWhenSchemeIsVersion1()
    {
        var input = new Scheme
        {
            ObjectCount = 10,
            Version = SchemeVersion.Version1
        };
        _writer.Write(input, _file);
        var result = _reader.Read(_file);

        result.Extended.FiringPausesTimer.ShouldBe(true);
    }

    private static IEnumerable<TestCaseData> Schemes => TestSchemes.Schemes();

    public void Dispose()
    {
        File.Delete(_file);
        Directory.Delete(_tempDirectory);
    }
}
