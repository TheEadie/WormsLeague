using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files.Tests.Schemes
{
    public class SchemeTextRoundTripShould
    {
        private SchemeTextWriter _writer;
        private SchemeTextReader _reader;

        [SetUp]
        public void SetUp()
        {
            _writer = new SchemeTextWriter();
            _reader = new SchemeTextReader();
        }

        [Test, TestCaseSource(nameof(Schemes))]
        public void NotLoseInformation(Scheme input)
        {
            var result = RoundTrip(input);
            result.ShouldBe(input);
        }

        private Scheme RoundTrip(Scheme scheme)
        {
            var output = new StringWriter();
            _writer.Write(scheme, output);
            var input = output.ToString();
            var result = _reader.GetModel(input);
            return result;
        }

        private static IEnumerable<TestCaseData> Schemes => TestSchemes.Schemes();
    }
}
