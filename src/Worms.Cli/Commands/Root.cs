using System.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Root : RootCommand
{
    public Root() : base("Worms CLI")
    {
        Options.Add(new Option<bool>("--verbose", "-v") { Description = "Show more information about the process" });
        Options.Add(new Option<bool>("--quiet", "-q") { Description = "Only show errors" });
    }
}
