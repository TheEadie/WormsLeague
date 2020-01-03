using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Worms.Components.Repositories;
using Worms.Components.Updates;
using Worms.GameRunner;

namespace Worms.Components
{
    public class Game : IComponent
    {
        private readonly IWormsLocator _wormsLocator;

        public string Name { get; }
        
        public Version InstalledVersion => _wormsLocator.Find().Version;

        public string ComponentPath => _wormsLocator.Find().ExeLocation;

        public Game(IWormsLocator wormsLocator)
        {
            _wormsLocator = wormsLocator;

            Name = "Worms Armaggedon";
        }

        public Task<IReadOnlyCollection<Version>> GetAvailiableVersions()
        {
            // https://worms2d.info/List_of_Worms_Armageddon_logic_versions
            var knownVersions = new List<Version> { new Version(3, 7, 2, 1) };
            return Task.FromResult(knownVersions as IReadOnlyCollection<Version>);
        }

        public Task Install(Version version)
        {
            // Use Steam
            return Task.CompletedTask;
        }
    }
}