using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;
using Worms.Components;

namespace Worms.Commands
{
    [Command("update", Description = "Update worms CLI")]
    public class Update : CommandBase
    {
        private readonly ComponentOperations _componentOperations;
        private readonly ComponentsRepository _componentsRepository;
        private readonly IFileSystem _fileSystem;

        public Update(
            ComponentOperations componentOperations,
            ComponentsRepository componentsRepository,
            IFileSystem fileSystem)
        {
            _componentOperations = componentOperations;
            _componentsRepository = componentsRepository;
            _fileSystem = fileSystem;
        }

        public async Task<int> OnExecuteAsync()
        {
            var components = _componentsRepository.GetAll();

            foreach(var component in components)
            {
                try
                {
                    await UpdateComponent(component);
                }
                catch (RateLimitExceededException)
                {
                    Logger.Error($"Could not update {component.Name}: GitHub API rate limit has been exceeded. Please run 'worms setup' and provide a personal access token.");
                    return 1;
                }
            }

            return 0;
        }

        private async Task UpdateComponent(Component component)
        {
            Logger.Verbose("Starting update");
            Logger.Verbose(component.ToString());

            var versions = await _componentOperations.GetAvailableVersions(component);

            Logger.Verbose($"Available versions: {string.Join(", ", versions)}");

            var latestVersion = versions.OrderByDescending(x => x).FirstOrDefault();
            Logger.Verbose($"Latest version: {latestVersion}");

            if (component.InstalledVersion > latestVersion)
            {
                Logger.Information($"{component.Name} is up to date: {component.InstalledVersion}");
                return;
            }

            Logger.Information($"Downloading {component.Name} {latestVersion}");
            await _componentOperations.Install(component, latestVersion);

            // TODO introduce class to hold this location per operating system
            var updateFolder = _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Worms", ".update");
            var updateScriptLocation = _fileSystem.Path.Combine(updateFolder, "UpdateCli.ps1");
            Logger.Information($"To install {component.Name} {latestVersion} run {updateScriptLocation}");
        }
    }
}