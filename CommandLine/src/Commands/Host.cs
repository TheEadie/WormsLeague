using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon;

namespace Worms.Commands
{
    [Command("host", Description="Host a game of worms using the latest options")]
    internal class Host : CommandBase
    {
        private readonly IWormsRunner _wormsRunner;

        public Host(IWormsRunner wormsRunner)
        {
            _wormsRunner = wormsRunner;
        }

        public async Task<int> OnExecuteAsync()
        {
            await _wormsRunner.RunWorms("wa://").ConfigureAwait(false);
            return 0;
        }
    }
}