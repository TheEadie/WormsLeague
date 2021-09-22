using System;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Gifs
{
    [Command("gif", Description = "Create animated gifs of replays (.gif files)")]
    internal class CreateGif : CommandBase
    {
        private readonly IResourceCreator<LocalGifCreateParameters> _gifCreator;
        private readonly IResourceRetriever<LocalReplay> _replayRetriever;

        [Argument(0, Name = "name", Description = "The name of the Scheme to be created")]
        public string Name { get; }

        [Option(Description = "The replay name", ShortName = "r")]
        public string Replay { get; }

        [Option(Description = "The turn number", ShortName = "t")]
        public uint Turn { get; }

        [Option(Description = "The number of frames per second", ShortName = "fps")]
        public uint FramesPerSecond { get; } = 10;

        public CreateGif(IResourceCreator<LocalGifCreateParameters> gifCreator, IResourceRetriever<LocalReplay> replayRetriever)
        {
            _gifCreator = gifCreator;
            _replayRetriever = replayRetriever;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            string name;
            LocalReplay replay;

            try
            {
                name = ValidateName();
                replay = ValidateReplay(Replay, Turn);

            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            try
            {
                await _gifCreator.Create(new LocalGifCreateParameters(name, string.Empty, replay, Turn, FramesPerSecond));
            }
            catch (FormatException exception)
            {
                Logger.Error("Failed to read Scheme definition: " + exception.Message);
                return 1;
            }

            return 0;
        }

        private LocalReplay ValidateReplay(string replay, uint turn)
        {
            if (string.IsNullOrWhiteSpace(replay))
            {
                throw new ConfigurationException("No replay provided for the Gif being created");
            }
            if (turn == default)
            {
                throw new ConfigurationException("No turn provided for the Gif being created");
            }

            var replays = _replayRetriever.Get(replay);

            switch (replays.Count)
            {
                case 0:
                    throw new ConfigurationException($"No replays found with name: {replay}");
                case > 1:
                    throw new ConfigurationException($"More than one replay found matching pattern: {replay}");
            }

            var foundReplay = replays.Single();
            if (foundReplay.Details.Turns.Count < turn)
            {
                throw new ConfigurationException($"Replay {replay} does not have a turn {turn}");
            }

            return foundReplay;
        }

        private string ValidateName()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            throw new ConfigurationException("No name provided for the Scheme being created.");
        }
    }
}
