using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Components;
using Worms.Components.Updates;

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

        public Task<int> OnExecuteAsync(IConsole console)
        {
            foreach(var component in _components)
            {
                var latestVersion = component.GetAvailiableVersions().OrderByDescending(x => x).First();
                if (component.InstalledVersion > latestVersion)
                {
                    break;
                }

                component.Install(latestVersion);
                console.WriteLine($"Updated {component.Name} to {latestVersion}");
            }

            return Task.FromResult(0);
        }
    }
}