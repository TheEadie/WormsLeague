using System.Collections.Generic;
using WormsRandomizer.Config;

namespace WormsRandomizer.Flags
{
    internal interface IFlagParser
    {
        void ParseFlag(string arg, SchemeRandomizerConfig schemeConfig);
        IEnumerable<string> GetFlagsHelp();
    }
}