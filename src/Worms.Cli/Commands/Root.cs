using System.CommandLine;

namespace Worms.Cli.Commands
{
    internal class Root : RootCommand
    {
        public Root() : base("Worms CLI")
        {
            AddGlobalOption(new Option<bool>(
                new[] { "--verbose", "-v" },
                "Show more information about the process"));
            AddGlobalOption(new Option<bool>(
                new[] { "--quiet", "-q" },
                "Only show errors"));
        }
    }
}
