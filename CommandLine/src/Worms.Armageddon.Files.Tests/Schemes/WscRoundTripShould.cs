using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Resources.Schemes.Binary;

namespace Worms.Armageddon.Resources.Tests.Schemes
{
    public class WscRoundTripShould
    {
        private IWscWriter _writer;
        private IWscReader _reader;
        private string _tempDirectory;
        private string _file;

        [SetUp]
        public void SetUp()
        {
            _writer = new WscWriter();
            _reader = new WscReader();

            _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _file = Path.Combine(_tempDirectory, Path.GetRandomFileName() + ".wsc");

            Directory.CreateDirectory(_tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_file);
            Directory.Delete(_tempDirectory);
        }

        [Test, TestCaseSource(nameof(Schemes))]
        public void NotLoseInformation(Scheme input)
        {
            _writer.Write(input, _file);
            var result = _reader.Read(_file);
            result.ShouldBe(input);
        }

        [Test]
        // This setting has the wrong default value in the underlying library
        public void SetFiringPausesTimerToTrue_WhenSchemeIsVersion1()
        {
            var input = new Scheme{ ObjectCount = 10, Version = SchemeVersion.Version1 };
            _writer.Write(input, _file);
            var result = _reader.Read(_file);

            result.Extended.FiringPausesTimer.ShouldBe(true);
        }

        private static IEnumerable<TestCaseData> Schemes => TestSchemes.Schemes();
    }
}
