using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Cli.Resources.Local.Replays;
using Worms.Resources;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "Delete replays (.WAgame file)")]
    internal class DeleteReplay : CommandBase
    {
        private readonly ResourceDeleter<LocalReplay> _resourceDeleter;

        [Argument(0, Name = "name", Description = "The name of the Replay to be deleted")]
        public string Name { get; }

        public DeleteReplay(ResourceDeleter<LocalReplay> resourceDeleter)
        {
            _resourceDeleter = resourceDeleter;
        }

        public async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _resourceDeleter.Delete(Name, Logger, cancellationToken);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            return 0;
        }
    }
}
