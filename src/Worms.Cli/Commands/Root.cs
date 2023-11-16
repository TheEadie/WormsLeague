using System.CommandLine;

namespace Worms.Cli.Commands;

internal sealed class Root : RootCommand
{
    private static readonly string[] VerboseArgs =
    {
        "--verbose",
        "-v"
    };

    private static readonly string[] QuietArgs =
    {
        "--quiet",
        "-q"
    };

    public Root()
        : base("Worms CLI")
    {
        AddGlobalOption(new Option<bool>(VerboseArgs, "Show more information about the process"));
        AddGlobalOption(new Option<bool>(QuietArgs, "Only show errors"));
    }
}
