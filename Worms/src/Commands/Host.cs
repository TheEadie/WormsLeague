using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Worms.Commands
{
    [Command("host", Description="Host a game of worms using the latest options")]
    public class Host 
    {
        public Task<int> OnExecuteAsync()
        {
            // TODO: Download latest options from a repo if configured
            // Launch worms
            // TODO: Notify to any channels configured
            return Task.FromResult(0);
        }
    }
}