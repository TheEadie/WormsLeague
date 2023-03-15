using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Commands.Resources.Schemes
{
    internal class CreateScheme : Command
    {
        public static readonly Argument<string> SchemeName = new(
            "name",
            "The name of the Scheme to be created");

        public static readonly Option<string> InputFile = new(
            new[] {"--file", "-f"},
            "File to load the Scheme definition from");

        public static readonly Option<string> ResourceFolder = new(
            new[] {"--resource-folder", "-r"},
            "Override the folder that the Scheme will be created in");

        public static readonly Option<bool> Random = new(
            new[] {"--random"},
            "Generate a scheme randomnly");

        public CreateScheme() : base("scheme", "Create Worms Schemes (.wsc files)")
        {
            AddAlias("schemes");
            AddAlias("wsc");
        }
    }

    // ReSharper disable once UnusedType.Global
    internal class CreateSchemeHandler : ICommandHandler
    {
        private readonly IResourceCreator<LocalScheme, LocalSchemeCreateParameters> _schemeCreator;
        private readonly IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters> _randomSchemeCreator;
        private readonly IFileSystem _fileSystem;
        private readonly IWormsLocator _wormsLocator;
        private readonly ILogger _logger;

        public CreateSchemeHandler(IResourceCreator<LocalScheme, LocalSchemeCreateParameters> schemeCreator,
            IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters> randomSchemeCreator,
            IFileSystem fileSystem,
            IWormsLocator wormsLocator,
            ILogger logger)
        {
            _schemeCreator = schemeCreator;
            _randomSchemeCreator = randomSchemeCreator;
            _fileSystem = fileSystem;
            _wormsLocator = wormsLocator;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(CreateScheme.SchemeName);
            var resourceFolder = context.ParseResult.GetValueForOption(CreateScheme.ResourceFolder);
            var inputFile = context.ParseResult.GetValueForOption(CreateScheme.InputFile);
            var random = context.ParseResult.GetValueForOption(CreateScheme.Random);
            var cancellationToken = context.GetCancellationToken();

            var outputFolder = resourceFolder;
            Func<Task<LocalScheme>> creator;

            try
            {
                var validatedName = ValidateName(name);
                outputFolder = ValidateOutputFolder(outputFolder);
                if (!random)
                {
                    var (definition, source) = ValidateSchemeDefinition(inputFile);
                    creator = () =>
                        _schemeCreator.Create(new LocalSchemeCreateParameters(validatedName, outputFolder, definition),
                            _logger,
                            cancellationToken);
                    _logger.Verbose($"Scheme definition being read from {source}");
                }
                else
                {
                    creator = () =>
                        _randomSchemeCreator.Create(new LocalSchemeCreateRandomParameters(validatedName, outputFolder),
                            _logger,
                            cancellationToken);
                }
            }
            catch (ConfigurationException exception)
            {
                _logger.Error(exception.Message);
                return 1;
            }

            _logger.Information($"Writing Scheme to {outputFolder}");

            try
            {
                var scheme = await creator();
                await Console.Out.WriteLineAsync(scheme.Path);
            }
            catch (FormatException exception)
            {
                _logger.Error("Failed to read Scheme definition: " + exception.Message);
                return 1;
            }

            return 0;
        }

        private string ValidateOutputFolder(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
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

            _logger.Information($"Output folder ({outputFolder}) does not exit. It will be created.");
            _fileSystem.Directory.CreateDirectory(outputFolder);

            return outputFolder;
        }

        private (string, string) ValidateSchemeDefinition(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                return (_fileSystem.File.ReadAllText(filename), $"file: + {filename}");
            }

            if (Console.IsInputRedirected)
            {
                return (Console.In.ReadToEnd(), "std in");
            }

            throw new ConfigurationException(
                "No Scheme definition provided. Provide the definition using std in or the --file option");
        }

        private static string ValidateName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            throw new ConfigurationException("No name provided for the Scheme being created.");
        }
    }
}