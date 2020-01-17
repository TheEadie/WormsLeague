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

        public Update(
            ComponentOperations componentOperations,
            ComponentsRepository componentsRepository)
        {
            _componentOperations = componentOperations;
            _componentsRepository = componentsRepository;
        }

        public async Task<int> OnExecuteAsync()
        {
            foreach (var component in _componentsRepository.GetAll())
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
            Logger.Verbose("Checking for updates");
            Logger.Verbose(component.ToString());

            var versions = await _componentOperations.GetAvailableVersions(component);

            Logger.Verbose($"Available versions: {string.Join(", ", versions)}");

            var latestVersion = versions.OrderByDescending(x => x).FirstOrDefault();
            Logger.Verbose($"Current version: {component.InstalledVersion}");
            Logger.Verbose($"Latest version: {latestVersion}");

            if (component.InstalledVersion > latestVersion)
            {
                Logger.Information($"{component.Name} is up to date: {component.InstalledVersion}");
                return;
            }

            Logger.Information($"An update is availible. Downloading {component.Name} {latestVersion}");
            await _componentOperations.Install(component, latestVersion);
            Logger.Information($"{component.Name} {latestVersion} has been downloaded. Run `worms-update` to install.");
        }
    }
}