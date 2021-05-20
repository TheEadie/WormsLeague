using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Game.Replays;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "View replays (.WAgame file)")]
    internal class ViewReplay : CommandBase
    {
        private readonly IReplayPlayer _replayPlayer;
        private readonly IResourceRetriever<LocalReplay> _replayRetriever;

        [Argument(0, Name = "name", Description = "The name of the Replay to be viewed")]
        public string Name { get; }

        [Option(Description = "The turn you wish to start the replay from", ShortName = "t")]
        public uint Turn { get; }

        public ViewReplay(IReplayPlayer replayPlayer, IResourceRetriever<LocalReplay> replayRetriever)
        {
            _replayPlayer = replayPlayer;
            _replayRetriever = replayRetriever;
        }

        public async Task<int> OnExecuteAsync()
        {
            LocalReplay replay;

            try
            {
                var name = ValidateName();
                replay = GetReplay(name);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            if (Turn != default)
            {
                var startTime = replay.Details.Turns.ElementAt((int)Turn - 1).Start;
                await _replayPlayer.Play(replay.Paths.WAgamePath, startTime);
                return 0;
            }

            await _replayPlayer.Play(replay.Paths.WAgamePath);
            return 0;
        }

        private LocalReplay GetReplay(string name)
        {
            var replaysFound = _replayRetriever.Get(name);

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
