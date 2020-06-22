using System.IO.Abstractions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Resources.Schemes;
using Worms.WormsArmageddon;
using Worms.WormsArmageddon.Schemes.WscFiles;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("scheme", "schemes", Description = "Create Worms Schemes (.wsc files)")]
    internal class CreateScheme : CommandBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly WscWriter _wscWriter;
        private readonly IWormsLocator _wormsLocator;

        [Option(Description = "The name for the created Scheme", ShortName = "n")]
        public string Name { get; }

        [Option(Description = "The file to load the scheme definition from", ShortName = "f")]
        public string FilePath { get; }

        [Option(Description = "Override the location that the Scheme will be written to", ShortName = "s")]
        public string OutputFolder { get; }

        public CreateScheme(IFileSystem fileSystem, WscWriter wscWriter, IWormsLocator wormsLocator)
        {
            _fileSystem = fileSystem;
            _wscWriter = wscWriter;
            _wormsLocator = wormsLocator;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            string name;
            string definition;
            string source;
            var outputFolder = OutputFolder;

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

            Logger.Verbose($"Reading definition from {source}");
            var schemeReader = new SchemeTextReader();
            var scheme = schemeReader.GetModel(definition);

            var outputFilePath = _fileSystem.Path.Combine(outputFolder, name + ".wsc");
            Logger.Verbose($"Writing scheme to {outputFilePath}");
            _wscWriter.WriteModel(scheme, outputFilePath);

            return Task.FromResult(0);
        }

        private string ValidateOutputFolder(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                var gameInfo = _wormsLocator.Find();
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
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                return (_fileSystem.File.ReadAllText(FilePath), $"file: + {FilePath}");
            }

            if (console.IsInputRedirected)
            {
                return (console.In.ReadToEnd(), "std in");
            }

            throw new ConfigurationException("No scheme definition provided");
        }

        private string ValidateName()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            throw new ConfigurationException("No name provided for the scheme being created.");
        }
    }
}
