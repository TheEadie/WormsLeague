using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Logging;
using Worms.Resources.Schemes;

namespace Worms.Commands
{
    [Command("scheme", "schemes", Description = "List all Schemes known to Worms CLI")]
    internal class GetScheme : CommandBase
    {
        private readonly ISchemesRetriever _schemesRetreiver;
        private readonly TablePrinter _tablePrinter;
        private readonly TextPrinter _textPrinter;

        [Argument(0)]
        public string Name { get; }

        public GetScheme(
            ISchemesRetriever schemesRetreiver,
            TablePrinter tablePrinter,
            TextPrinter textPrinter)
        {
            _schemesRetreiver = schemesRetreiver;
            _tablePrinter = tablePrinter;
            _textPrinter = textPrinter;
        }

        public Task<int> OnExecuteAsync()
        {
            try
            {
                PrintScheme(Name);
                return Task.FromResult(0);
            }
            catch (ArgumentException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }
        }

        private void PrintScheme(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _tablePrinter.Print(Logger, _schemesRetreiver.Get());
            }
            else
            {
                _textPrinter.Print(Logger, _schemesRetreiver.Get(name));
            }
        }
    }
}