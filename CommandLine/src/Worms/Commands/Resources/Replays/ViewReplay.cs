using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Game.Replays;
using Worms.Armageddon.Resources.Replays;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Replays;

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
        private readonly IResourceRetriever<ReplayResource> _replayRetriever;

        [Argument(0, Name = "name", Description = "The name of the Replay to be viewed")]
        public string Name { get; }

        [Option(Description = "The turn you wish to start the replay from", ShortName = "t")]
        public uint Turn { get; }

        public ViewReplay(IReplayPlayer replayPlayer, IReplayLocator replayLocator, IResourceRetriever<ReplayResource> replayRetriever)
        {
            _replayPlayer = replayPlayer;
            _replayLocator = replayLocator;
            _replayRetriever = replayRetriever;
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

            if (Turn != default)
            {
                var replay = _replayRetriever.Get(Name).Single();
                var startTime = replay.Turns.ElementAt((int)Turn - 1).Start;
                await _replayPlayer.Play(filePaths.WAgamePath, startTime);
                return 0;
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
