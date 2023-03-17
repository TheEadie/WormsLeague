using System.CommandLine;

namespace Worms.Commands.Resources;

internal class Browse : Command
{
    public Browse() : base("browse", "Open the folder containing this resource")
    {
        AddAlias("dir");
        AddAlias("locate");
    }
}
