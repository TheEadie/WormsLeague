using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.GameRunner;

namespace Worms.Commands
{
    [Command("host", Description="Host a game of worms using the latest options")]
    public class Host 
    {
        private readonly IWormsRunner _wormsRunner;

        public Host(IWormsRunner wormsRunner)
        {
            _wormsRunner = wormsRunner;
        }

        public async Task<int> OnExecuteAsync()
        {
            await _wormsRunner.RunWorms("wa://");
            return 0;
        }
    }
}