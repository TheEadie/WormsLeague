using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Resources.Schemes;

namespace Worms.Commands
{
    [Command("scheme", "schemes", Description = "List all Schemes known to Worms CLI")]
    internal class GetScheme : CommandBase
    {
        private readonly ISchemesRetriever _schemesRetreiver;

        public GetScheme(ISchemesRetriever schemesRetreiver)
        {
            _schemesRetreiver = schemesRetreiver;
        }

        public Task<int> OnExecuteAsync()
        {
            foreach(var scheme in _schemesRetreiver.Get())
            {
                Logger.Information($"Name {scheme.Name}");
            }

            return Task.FromResult(0);
        }
    }
}