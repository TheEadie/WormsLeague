using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Components;

namespace Worms.Commands
{
    [Command("update", Description = "Update worms CLI")]
    public class Update
    {
        private readonly IEnumerable<IComponent> _components;

        public Update(IEnumerable<IComponent> components)
        {
            _components = components;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            foreach(var component in _components)
            {
                var versions = await component.GetAvailiableVersions();
                var latestVersion = versions.OrderByDescending(x => x).First();
                if (component.InstalledVersion > latestVersion)
                {
                    console.WriteLine($"{component.Name} is up to date: {latestVersion}");
                    break;
                }

                component.Install(latestVersion);
                console.WriteLine($"Updated {component.Name} to {latestVersion}");
            }

            return 0;
        }
    }
}