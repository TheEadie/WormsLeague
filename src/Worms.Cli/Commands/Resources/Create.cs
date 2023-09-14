using System.CommandLine;

namespace Worms.Cli.Commands.Resources;

internal class Create : Command
{
    public Create() : base("create", "Create a resource")
    {
    }
}