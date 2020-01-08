using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
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

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            var components = _componentsRepository.GetAll();

            foreach(var component in components)
            {
                var versions = await _componentOperations.GetAvailableVersions(component);
                var latestVersion = versions.OrderByDescending(x => x).First();
                if (component.InstalledVersion > latestVersion)
                {
                    console.WriteLine($"{component.Name} is up to date: {latestVersion}");
                    break;
                }

                await _componentOperations.Install(component, latestVersion);
                console.WriteLine($"Updated {component.Name} to {latestVersion}");
            }

            return 0;
        }
    }
}