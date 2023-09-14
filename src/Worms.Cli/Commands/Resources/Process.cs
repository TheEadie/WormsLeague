using System.CommandLine;

namespace Worms.Cli.Commands.Resources;

internal sealed class Process : Command
{
    public Process() : base("process", "Process a resource to extract more information")
    {
    }
}
