using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Worms.Components.Updaters;

namespace Worms.Components
{
    public class ComponentOperations
    {
        private readonly OutsideOfToolUpdater _outsideOfToolUpdater;
        private readonly GitHubReleaseUpdater _gitHubReleaseUpdater;

        public ComponentOperations(OutsideOfToolUpdater outsideOfToolUpdater, GitHubReleaseUpdater gitHubReleaseUpdater)
        {
            _outsideOfToolUpdater = outsideOfToolUpdater;
            _gitHubReleaseUpdater = gitHubReleaseUpdater;
        }

        public async Task<IReadOnlyCollection<Version>> GetAvailiableVersions(Component component)
        {
            switch(component.UpdateConfig)
            {
                case OutsideOfToolUpdateConfig config:
                    return await _outsideOfToolUpdater.GetAvailiableVersions(component, config);
                case GitHubReleaseUpdateConfig config:
                    return await _gitHubReleaseUpdater.GetAvailiableVersions(component, config);
                default:
                    throw new ArgumentOutOfRangeException($"Unknown UpdateConfig {typeof(UpdateConfig)}");
            }
        }

        public Task Install(Component component, Version version)
        {
            switch(component.UpdateConfig)
            {
                case OutsideOfToolUpdateConfig config:
                    return _outsideOfToolUpdater.Install(component, version, config);
                case GitHubReleaseUpdateConfig config:
                    return _gitHubReleaseUpdater.Install(component, version, config);
                default:
                    throw new ArgumentOutOfRangeException($"Unknown UpdateConfig {typeof(UpdateConfig)}");
            }
        }
    }
}