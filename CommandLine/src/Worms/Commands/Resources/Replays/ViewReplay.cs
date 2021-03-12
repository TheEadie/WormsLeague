using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "View replays (.WAgame file)")]
    internal class ViewReplay : CommandBase
    {
        private readonly IReplayPlayer _replayPlayer;
        private readonly IReplayLocator _replayLocator;

        [Argument(0, Name = "name", Description = "The name of the Replay to be viewed")]
        public string Name { get; }

        public ViewReplay(IReplayPlayer replayPlayer, IReplayLocator replayLocator)
        {
            _replayPlayer = replayPlayer;
            _replayLocator = replayLocator;
        }

        public async Task<int> OnExecuteAsync()
        {
            ReplayPaths filePaths;

            try
            {
                var name = ValidateName();
                filePaths = GetFilePaths(name);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            await _replayPlayer.Play(filePaths.WAgamePath);
            return 0;
        }

        private ReplayPaths GetFilePaths(string name)
        {
            var replaysFound = _replayLocator.GetReplayPaths(name);

            if (replaysFound.Count == 0)
            {
                throw new ConfigurationException($"No Replay found with name: {name}");
            }

            if (replaysFound.Count > 1)
            {
                throw new ConfigurationException($"More than one Replay found with name matching: {name}");
            }

            return replaysFound.Single();
        }

        private string ValidateName()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            throw new ConfigurationException("No name provided for the Replay to be viewed.");
        }
    }
}
