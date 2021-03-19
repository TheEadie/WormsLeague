using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Resources.Schemes.Binary;

namespace Worms.Armageddon.Resources.Tests.Schemes
{
    public class SchemeBinaryRoundTripShould
    {
        private IWscWriter _writer;
        private IWscReader _reader;

        [SetUp]
        public void SetUp()
        {
            _writer = new WscWriter();
            _reader = new WscReader();
        }

        [Test, TestCaseSource(nameof(Schemes))]
        public void NotLoseInformation(Scheme input)
        {
            var temporaryDirectory = GetTemporaryDirectory();
            var file = Path.Combine(temporaryDirectory, Path.GetRandomFileName() + ".wsc");

            _writer.Write(input, file);
            var result = _reader.Read(file);

            result.ShouldBe(input);

            File.Delete(file);
            Directory.Delete(temporaryDirectory);
        }

        private static IEnumerable<TestCaseData> Schemes => TestSchemes.Schemes();

        private string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

    }
}
