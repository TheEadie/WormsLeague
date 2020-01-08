using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Components.Updaters
{
    public interface IUpdater<T>
    {
        Task<IReadOnlyCollection<Version>> GetAvailiableVersions(Component component, T config);
        Task Install(Component component, Version version, T config);
    }
}