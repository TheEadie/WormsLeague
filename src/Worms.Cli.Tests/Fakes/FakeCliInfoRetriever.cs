using Worms.Cli.Resources;

namespace Worms.Cli.Tests.Fakes;

internal sealed class FakeCliInfoRetriever : ICliInfoRetriever
{
    public CliInfo Info { get; } = new(new Version(1, 0, 0), "/cli", "worms");

    public CliInfo GetCliInfo() => Info;
}
