using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Logging;
using Worms.Resources.Schemes;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("scheme", "schemes", Description = "Retrieves information for Worms Schemes (.wsc files)")]
    internal class GetScheme : CommandBase
    {
        private readonly ISchemesRetriever _schemesRetriever;
        private readonly IResourcePrinter<SchemeResource> _printer;

        [Argument(
            0,
            Description =
                "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported",
            Name = "name")]
        public string Name { get; }

        public GetScheme(ISchemesRetriever schemesRetriever, IResourcePrinter<SchemeResource> printer)
        {
            _schemesRetriever = schemesRetriever;
            _printer = printer;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            try
            {
                PrintScheme(Name, console.Out);
                return Task.FromResult(0);
            }
            catch (ArgumentException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }
        }

        private void PrintScheme(string name, TextWriter writer)
        {
            var requestForAll = string.IsNullOrWhiteSpace(name);
            var userSpecifiedName = !requestForAll && !name.Contains('*');
            var matches = requestForAll ? _schemesRetriever.Get() : _schemesRetriever.Get(name);

            if (userSpecifiedName)
            {
                switch (matches.Count)
                {
                    case 0:
                        Logger.Error($"No Scheme found with name: {name}");
                        break;
                    case 1:
                        _printer.Print(writer, matches.Single());
                        break;
                    default:
                        _printer.Print(writer, matches);
                        break;
                }
            }
            else
            {
                _printer.Print(writer, matches);
            }
        }
    }
}
