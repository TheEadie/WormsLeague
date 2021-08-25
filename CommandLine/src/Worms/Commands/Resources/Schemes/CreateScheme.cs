using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Game;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Schemes;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands.Resources.Schemes
{
    [Command("scheme", "schemes", "wsc", Description = "Create Worms Schemes (.wsc files)")]
    internal class CreateScheme : CommandBase
    {
        private readonly IResourceCreator<LocalSchemeCreateParameters> _schemeCreator;
        private readonly IFileSystem _fileSystem;
        private readonly IWormsLocator _wormsLocator;

        [Argument(0, Name = "name", Description = "The name of the Scheme to be created")]
        public string Name { get; }

        [Option(Description = "File to load the Scheme definition from", ShortName = "f")]
        public string File { get; }

        [Option(Description = "Override the folder that the Scheme will be created in", ShortName = "r")]
        public string ResourceFolder { get; }

        public CreateScheme(IResourceCreator<LocalSchemeCreateParameters> schemeCreator, IFileSystem fileSystem, IWormsLocator wormsLocator)
        {
            _schemeCreator = schemeCreator;
            _fileSystem = fileSystem;
            _wormsLocator = wormsLocator;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            string name;
            string definition;
            string source;
            var outputFolder = ResourceFolder;

            try
            {
                name = ValidateName();
                (definition, source) = ValidateSchemeDefinition(console);
                outputFolder = ValidateOutputFolder(outputFolder);
            }
            catch (ConfigurationException exception)
            {
                Logger.Error(exception.Message);
                return Task.FromResult(1);
            }

            Logger.Verbose($"Scheme definition being read from {source}");
            Logger.Information($"Writing Scheme to {outputFolder}");

            try
            {
                _schemeCreator.Create(new LocalSchemeCreateParameters(name, outputFolder, definition));
            }
            catch (FormatException exception)
            {
                Logger.Error("Failed to read Scheme definition: " + exception.Message);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }

        private string ValidateOutputFolder(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(ResourceFolder))
            {
                var gameInfo = _wormsLocator.Find();

                if (!gameInfo.IsInstalled)
                {
                    throw new ConfigurationException(
                        "Worms is not installed. Use the --resource-folder option to specify where the Scheme should be created");
                }

                outputFolder = gameInfo.SchemesFolder;
            }

            if (_fileSystem.Directory.Exists(outputFolder))
            {
                return outputFolder;
            }

            Logger.Information($"Output folder ({outputFolder}) does not exit. It will be created.");
            _fileSystem.Directory.CreateDirectory(outputFolder);

            return outputFolder;
        }

        private (string, string) ValidateSchemeDefinition(IConsole console)
        {
            if (!string.IsNullOrWhiteSpace(File))
            {
                return (_fileSystem.File.ReadAllText(File), $"file: + {File}");
            }

            if (console.IsInputRedirected)
            {
                return (console.In.ReadToEnd(), "std in");
            }

            throw new ConfigurationException("No Scheme definition provided. Provide the definition using std in or the --file option");
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
