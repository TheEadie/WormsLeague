using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Resources.Schemes;
using Worms.Resources;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Schemes
{
    [Command("scheme", "schemes", "wsc", Description = "Delete Worms Schemes (.wsc files)")]
    internal class DeleteScheme : CommandBase
    {
        private readonly ResourceDeleter<SchemeResource> _resourceDeleter;

        [Argument(0, Name = "name", Description = "The name of the Scheme to be deleted")]
        public string Name { get; }

        public DeleteScheme(ResourceDeleter<SchemeResource> resourceDeleter)
        {
            _resourceDeleter = resourceDeleter;
        }

        public Task<int> OnExecuteAsync(IConsole console)
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
