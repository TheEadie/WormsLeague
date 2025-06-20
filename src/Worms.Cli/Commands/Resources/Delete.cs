using System.CommandLine;

namespace Worms.Cli.Commands.Resources;

internal sealed class Delete : Command
{
    public Delete()
        : base("delete", "Delete a resource") =>
        Aliases.Add("rm");
}
