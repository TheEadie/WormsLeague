using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("get", Description = "Get a list of resources"),
        Subcommand(typeof(GetScheme)),
    ]
    internal class Get : CommandBase
    {
        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            Logger.Error("No resource type specified");
            Logger.Information("");
            app.ShowHelp();
            return Task.FromResult(1);
        }
    }
}
