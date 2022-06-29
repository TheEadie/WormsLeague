using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Resources;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Schemes
{
    [Command("scheme", "schemes", "wsc", Description = "Retrieves information for Worms Schemes (.wsc files)")]
    internal class GetScheme : CommandBase
    {
        private readonly ResourceGetter<LocalScheme> _schemesRetriever;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported")]
        public string Name { get; }

        public GetScheme(ResourceGetter<LocalScheme> schemesRetriever)
        {
            _schemesRetriever = schemesRetriever;
        }

        public async Task<int> OnExecuteAsync(IConsole console, CancellationToken cancellationToken)
        {
            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                await _schemesRetriever.PrintResources(Name, console.Out, windowWidth, Logger, cancellationToken);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            return 0;
        }
    }
}
