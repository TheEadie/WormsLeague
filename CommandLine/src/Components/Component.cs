using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Worms.Components.Repositories;

namespace Worms.Components
{
    public class Component
    {
        public string Name { get; }
        public Version InstalledVersion { get; }
        public string ComponentPath { get; }
        public UpdateConfig UpdateConfig { get; }

        public Component(
            string name,
            Version installedVersion,
            string componentPath,
            UpdateConfig updateConfig)
        {
            Name = name;
            InstalledVersion = installedVersion;
            ComponentPath = componentPath;
            UpdateConfig = updateConfig;
        }
    }
}