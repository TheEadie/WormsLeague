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

        public GetScheme(ISchemesRetriever schemesRetreiver, TablePrinter tablePrinter)
        {
            _schemesRetreiver = schemesRetreiver;
            _tablePrinter = tablePrinter;
        }

        public Task<int> OnExecuteAsync()
        {
            _tablePrinter.Print(Logger, _schemesRetreiver.Get());

            return Task.FromResult(0);
        }
    }
}