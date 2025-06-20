using System.CommandLine;

namespace Worms.Cli.Commands.Resources;

internal sealed class Browse : Command
{
    public Browse()
        : base("browse", "Open the folder containing this resource")
    {
        Aliases.Add("dir");
        Aliases.Add("locate");
    }
}
