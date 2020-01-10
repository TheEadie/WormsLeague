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
            var latestVersion = versions.OrderByDescending(x => x).FirstOrDefault();
            if (component.InstalledVersion > latestVersion)
            {
                Logger.Information($"{component.Name} is up to date: {latestVersion}");
                return;
            }

            await _componentOperations.Install(component, latestVersion);
            Logger.Information($"Updated {component.Name} to {latestVersion}");
        }
    }
}