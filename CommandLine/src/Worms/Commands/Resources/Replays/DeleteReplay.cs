using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Resources;
using Worms.Resources.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "Delete replays (.WAgame file)")]
    internal class DeleteReplay : CommandBase
    {
        private readonly ResourceDeleter<ReplayResource> _resourceDeleter;

        [Argument(0, Name = "name", Description = "The name of the Replay to be deleted")]
        public string Name { get; }

        public DeleteReplay(ResourceDeleter<ReplayResource> resourceDeleter)
        {
            _resourceDeleter = resourceDeleter;
        }

        public Task<int> OnExecuteAsync()
        {
            try
            {
                _resourceDeleter.Delete(Name);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }
    }
}
