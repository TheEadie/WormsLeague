using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Logging;
using Worms.Resources;
using Worms.Resources.Schemes;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("scheme", "schemes", "wsc", Description = "Retrieves information for Worms Schemes (.wsc files)")]
    internal class GetScheme : CommandBase
    {
        private readonly IResourceRetriever<SchemeResource> _schemesRetriever;
        private readonly IResourcePrinter<SchemeResource> _printer;

        [Argument(
            0,
            Name = "name",
            Description =
                "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported")]
        public string Name { get; }

        public GetScheme(IResourceRetriever<SchemeResource> schemesRetriever, IResourcePrinter<SchemeResource> printer)
        {
            _schemesRetriever = schemesRetriever;
            _printer = printer;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                PrintScheme(Name, console.Out, windowWidth);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }

        private void PrintScheme(string name, TextWriter writer, int outputMaxWidth)
        {
            var requestForAll = string.IsNullOrWhiteSpace(name);
            var userSpecifiedName = !requestForAll && !name.Contains('*');
            var matches = requestForAll ? _schemesRetriever.Get() : _schemesRetriever.Get(name);

            if (userSpecifiedName)
            {
                switch (matches.Count)
                {
                    case 0:
                        throw new ConfigurationException($"No Scheme found with name: {name}");
                    case 1:
                        _printer.Print(writer, matches.Single(), outputMaxWidth);
                        break;
                    default:
                        _printer.Print(writer, matches, outputMaxWidth);
                        break;
                }
            }
            else
            {
                _printer.Print(writer, matches, outputMaxWidth);
            }
        }
    }
}
