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
            if (string.IsNullOrWhiteSpace(Name))
            {
                _tablePrinter.Print(Logger, _schemesRetreiver.Get());
            }
            else
            {
                _textPrinter.Print(Logger, _schemesRetreiver.Get(Name));
            }

            return Task.FromResult(0);
        }
    }
}