using System;
using System.Collections.Generic;

namespace Worms.Components
{
    public class OutsideOfToolUpdateConfig : UpdateConfig

    {
        public IReadOnlyCollection<Version> PossibleVersions { get; }

        public OutsideOfToolUpdateConfig(IReadOnlyCollection<Version> possibleVersions)
        {
            PossibleVersions = possibleVersions;
        }
    }
}