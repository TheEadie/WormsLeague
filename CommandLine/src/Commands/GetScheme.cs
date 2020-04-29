using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Logging;
using Worms.Resources.Schemes;

namespace Worms.Commands
{
    [Command("scheme", "schemes", Description = "Retrieves information for Worms Schemes (.wsc files)")]
    internal class GetScheme : CommandBase
    {
        private readonly ISchemesRetriever _schemesRetriever;
        private readonly TablePrinter _tablePrinter;
        private readonly TextPrinter _textPrinter;

        [Argument(0, Description = "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported", Name="name")]
        public string Name { get; }

        public GetScheme(
            ISchemesRetriever schemesRetriever,
            TablePrinter tablePrinter,
            TextPrinter textPrinter)
        {
            _schemesRetriever = schemesRetriever;
            _tablePrinter = tablePrinter;
            _textPrinter = textPrinter;
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
            var matches = requestForAll ? _schemesRetriever.Get("*") : _schemesRetriever.Get(name);

            if (userSpecifiedName)
            {
                if (matches.Count == 0)
                {
                    Logger.Error($"No Scheme found with name: {name}");
                }
                else if (matches.Count == 1)
                {
                    _textPrinter.Print(writer, matches.Single());
                }
                else
                {
                    _tablePrinter.Print(writer, matches);
                }
            }
            else
            {
                _tablePrinter.Print(writer, matches);
            }
        }
    }
}
