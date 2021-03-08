using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon.Replays;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Replays
{
    [Command("replay", "replays", "WAgame", Description = "Delete replays (.WAgame file)")]
    internal class DeleteReplay : CommandBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IReplayLocator _replayLocator;

        [Argument(0, Name = "name", Description = "The name of the Replay to be deleted")]
        public string Name { get; }

        public DeleteReplay(IFileSystem fileSystem, IReplayLocator replayLocator)
        {
            _fileSystem = fileSystem;
            _replayLocator = replayLocator;
        }

        public Task<int> OnExecuteAsync()
        {
            ReplayPaths filePaths;

            try
            {
                var name = ValidateName();
                filePaths = GetFilePath(name);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }

            _fileSystem.File.Delete(filePaths.WAgamePath);
            if (!string.IsNullOrEmpty(filePaths.LogPath))
            {
                _fileSystem.File.Delete(filePaths.LogPath);
            }

            return Task.FromResult(0);
        }

        private ReplayPaths GetFilePath(string name)
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

            throw new ConfigurationException("No name provided for the Replay to be deleted.");
        }
    }
}
