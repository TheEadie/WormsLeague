using JetBrains.Annotations;
using Worms.Cli.CommandLine;

namespace Worms.Cli.Tests.Fakes;

internal sealed class FakeCliInfoRetriever : ICliInfoRetriever
{
    [PublicAPI] public CliInfo Info { get; set; } = new(new Version(1, 0, 0), "/cli", "worms");

    public CliInfo Get() => Info;
}
