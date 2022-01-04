using System;
using System.IO;
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
        private readonly IResourceCreator<LocalGif, LocalGifCreateParameters> _gifCreator;
        private readonly IResourceRetriever<LocalReplay> _replayRetriever;

        [Option(Description = "The replay name", ShortName = "r")]
        public string Replay { get; }

        [Option(Description = "The turn number", ShortName = "t")]
        public uint Turn { get; }

        [Option(Description = "The number of frames per second", ShortName = "fps")]
        public uint FramesPerSecond { get; } = 5;

        [Option(Description = "Speed multiplier for the gif", ShortName = "s")]
        public uint Speed { get; } = 2;

        [Option(Description = "Offset for the start of the gif in seconds", ShortName = "so")]
        public uint StartOffset { get; } = 0;

        [Option(Description = "Offset for the end of the gif in seconds", ShortName = "eo")]
        public uint EndOffset { get; } = 0;

        public CreateGif(IResourceCreator<LocalGif, LocalGifCreateParameters> gifCreator, IResourceRetriever<LocalReplay> replayRetriever)
        {
            _gifCreator = gifCreator;
            _replayRetriever = replayRetriever;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            LocalReplay replay;

            try
            {
                replay = ValidateReplay(Replay, Turn);

            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return 1;
            }

            try
            {
                Logger.Information($"Creating gif for {Replay}, turn {Turn} ...");
                var gif = await _gifCreator.Create(new LocalGifCreateParameters(replay, Turn,
                    TimeSpan.FromSeconds(StartOffset), TimeSpan.FromSeconds(EndOffset),
                    FramesPerSecond, Speed));
                await console.Out.WriteLineAsync(gif.Path);
            }
            catch (FormatException exception)
            {
                Logger.Error("Failed to create gif: " + exception.Message);
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
    }
}
