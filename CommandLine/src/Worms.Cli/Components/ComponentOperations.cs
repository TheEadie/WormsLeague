using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Worms.Components.Updaters;
using Worms.Components.Updaters.GitHubReleaseUpdater;
using Worms.Components.Updaters.OutsideOfToolUpdater;

namespace Worms.Components
{
    public class ComponentOperations
    {
        private readonly IUpdater<OutsideOfToolUpdateConfig> _outsideOfToolUpdater;
        private readonly IUpdater<GitHubReleaseUpdateConfig> _gitHubReleaseUpdater;

        public ComponentOperations(IUpdater<OutsideOfToolUpdateConfig> outsideOfToolUpdater, IUpdater<GitHubReleaseUpdateConfig> gitHubReleaseUpdater)
        {
            _outsideOfToolUpdater = outsideOfToolUpdater;
            _gitHubReleaseUpdater = gitHubReleaseUpdater;
        }

        public async Task<IReadOnlyCollection<Version>> GetAvailableVersions(Component component)
        {
            switch(component.UpdateConfig)
            {
                case OutsideOfToolUpdateConfig config:
                    return await _outsideOfToolUpdater.GetAvailableVersions(component, config);
                case GitHubReleaseUpdateConfig config:
                    return await _gitHubReleaseUpdater.GetAvailableVersions(component, config);
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