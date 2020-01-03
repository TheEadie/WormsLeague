using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Components
{
    public interface IComponent
    {
        string Name { get; }
        Version InstalledVersion { get; }
        string ComponentPath { get; }

        Task<IReadOnlyCollection<Version>> GetAvailiableVersions();
        Task Install(Version version);
    }
}