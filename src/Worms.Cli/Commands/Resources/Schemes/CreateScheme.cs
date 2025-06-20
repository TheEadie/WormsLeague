using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Cli.Commands.Resources.Schemes;

internal sealed class CreateScheme : Command
{
    public static readonly Argument<string> SchemeName =
        new("name") { Description = "The name of the Scheme to be created" };

    public static readonly Option<string> InputFile =
        new("--file", "-f") { Description = "File to load the Scheme definition from" };

    public static readonly Option<string> ResourceFolder = new("--resource-folder", "-r")
    {
        Description = "Override the folder that the Scheme will be created in"
    };

    public static readonly Option<bool> Random = new("--random") { Description = "Generate a scheme randomly" };

    public CreateScheme()
        : base("scheme", "Create Worms Schemes (.wsc files)")
    {
        Aliases.Add("schemes");
        Aliases.Add("wsc");

        Arguments.Add(SchemeName);
        Options.Add(InputFile);
        Options.Add(ResourceFolder);
        Options.Add(Random);
    }
}

internal sealed class CreateSchemeHandler(
    IResourceCreator<LocalScheme, LocalSchemeCreateParameters> schemeCreator,
    IFileSystem fileSystem,
    IWormsArmageddon wormsArmageddon,
    ILogger<CreateSchemeHandler> logger) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Scheme.SpanNameCreate);

        var config = new Config(
            parseResult.GetRequiredValue(CreateScheme.SchemeName),
            parseResult.GetValue(CreateScheme.ResourceFolder),
            parseResult.GetValue(CreateScheme.InputFile),
            parseResult.GetValue(CreateScheme.Random),
            wormsArmageddon.FindInstallation());

        var createParams = config.Validate(ValidConfig())
            .Map(x => x with { OutputFolder = x.GameInfo.IsInstalled ? x.GameInfo.SchemesFolder : x.OutputFolder })
            .Map(
                x => new LocalSchemeCreateParameters(
                    x.Name,
                    CreateFolderIfDoesNotExist(x.OutputFolder!),
                    x.Random,
                    LoadSchemeDefinition(x)));

        if (!createParams.IsValid)
        {
            createParams.LogErrors(logger);
            return 1;
        }

        logger.LogInformation("Writing Scheme to {Folder}", createParams.Value.Folder);
        var scheme = await schemeCreator.Create(createParams.Value, cancellationToken);
        await Console.Out.WriteLineAsync(scheme.Path);
        return 0;
    }

    private sealed record Config(string Name, string? OutputFolder, string? InputFile, bool Random, GameInfo GameInfo);

    private static List<ValidationRule<Config>> ValidConfig() =>
        Valid.Rules<Config>()
            .Must(x => !string.IsNullOrWhiteSpace(x.Name), "No name provided for the Scheme being created.")
            .Must(
                x => x.GameInfo.IsInstalled || !string.IsNullOrWhiteSpace(x.OutputFolder),
                "Worms is not installed. Use the --resource-folder option to specify where the Scheme should be created");

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    private string? LoadSchemeDefinition(Config config)
    {
        if (config.Random)
        {
            logger.LogDebug("Scheme definition being read from {Source}", "random");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(config.InputFile))
        {
            logger.LogDebug("Scheme definition being read from file: {FilePath}", config.InputFile);
            return fileSystem.File.ReadAllText(config.InputFile);
        }

        if (Console.IsInputRedirected)
        {
            logger.LogDebug("Scheme definition being read from {Source}", "std in");
            return Console.In.ReadToEnd();
        }

        throw new ArgumentException(
            "No Scheme definition provided. Provide the definition using std in or the --file option");
    }

    private string CreateFolderIfDoesNotExist(string outputFolder)
    {
        if (fileSystem.Directory.Exists(outputFolder))
        {
            return outputFolder;
        }

        logger.LogInformation("Output folder ({Folder}) does not exit. It will be created.", outputFolder);
        _ = fileSystem.Directory.CreateDirectory(outputFolder);

        return outputFolder;
    }
}
