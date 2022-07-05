using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Resources;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Schemes
{
    [Command("scheme", "schemes", "wsc", Description = "Delete Worms Schemes (.wsc files)")]
    internal class DeleteScheme : CommandBase
    {
        private readonly ResourceDeleter<LocalScheme> _resourceDeleter;

        [Argument(0, Name = "name", Description = "The name of the Scheme to be deleted")]
        public string Name { get; }

        public DeleteScheme(ResourceDeleter<LocalScheme> resourceDeleter)
        {
            _resourceDeleter = resourceDeleter;
        }

        public async Task<int> OnExecuteAsync(IConsole console, CancellationToken cancellationToken)
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
