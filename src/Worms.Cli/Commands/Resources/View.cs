using System.CommandLine;

namespace Worms.Cli.Commands.Resources;

internal sealed class View : Command
{
    public View() : base("view", "View a resource") { }
}
