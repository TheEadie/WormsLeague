using System.CommandLine;

namespace Worms.Cli.Commands.Resources;

internal sealed class Get : Command
{
    public Get()
        : base("get", "Get a list of resources") { }
}
