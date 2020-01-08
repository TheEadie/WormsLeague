using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Components.Updaters
{
    public class OutsideOfToolUpdater : IUpdater<OutsideOfToolUpdateConfig>
    {
        public Task<IReadOnlyCollection<Version>> GetAvailiableVersions(Component component, OutsideOfToolUpdateConfig config)
        {
            return Task.FromResult(config.PossibleVersions);
        }

        public Task Install(Component component, Version version, OutsideOfToolUpdateConfig config)
        {
            // No Op - Use external tool
            return Task.CompletedTask;
        }
    }
}