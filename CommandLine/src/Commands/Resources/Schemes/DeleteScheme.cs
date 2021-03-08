using System.IO.Abstractions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.WormsArmageddon;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Schemes
{
    [Command("scheme", "schemes", "wsc", Description = "Delete Worms Schemes (.wsc files)")]
    internal class DeleteScheme : CommandBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IWormsLocator _wormsLocator;

        [Argument(0, Name = "name", Description = "The name of the Scheme to be deleted")]
        public string Name { get; }

        public DeleteScheme(IFileSystem fileSystem, IWormsLocator wormsLocator)
        {
            _fileSystem = fileSystem;
            _wormsLocator = wormsLocator;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            string filePath;

            try
            {
                var name = ValidateName();
                filePath = GetFilePath(name);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }

            _fileSystem.File.Delete(filePath);
            return Task.FromResult(0);
        }

        private string GetFilePath(string name)
        {
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                throw new ConfigurationException(
                    "Worms is not installed.");
            }

            var filePath = _fileSystem.Path.Combine(gameInfo.SchemesFolder, name + ".wsc");

            if (!_fileSystem.File.Exists(filePath))
            {
                throw new ConfigurationException($"No Scheme file found with name: {name}");
            }

            return filePath;
        }

        private string ValidateName()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            throw new ConfigurationException("No name provided for the Scheme to be deleted.");
        }
    }
}
