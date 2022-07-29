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
    [Command("replay", "replays", "WAgame", Description = "View replays (.WAgame file)")]
    internal class ViewReplay : CommandBase
    {
        private readonly ResourceViewer<LocalReplay, LocalReplayViewParameters> _resourceViewer;

        [Argument(0, Name = "name", Description = "The name of the Replay to be viewed")]
        public string Name { get; }

        [Option(Description = "The turn you wish to start the replay from", ShortName = "t")]
        public uint Turn { get; }

        public ViewReplay(ResourceViewer<LocalReplay, LocalReplayViewParameters> resourceViewer)
        {
            _resourceViewer = resourceViewer;
        }

        public async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _resourceViewer.View(Name, new LocalReplayViewParameters(Turn), Logger, cancellationToken);
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
