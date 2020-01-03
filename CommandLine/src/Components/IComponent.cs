using System;
using System.Collections.Generic;

namespace Worms.Components
{
    public interface IComponent
    {
        string Name { get; }
        Version InstalledVersion { get; }
        string ComponentPath { get; }

        IReadOnlyCollection<Version> GetAvailiableVersions();
        void Install(Version version);
    }
}