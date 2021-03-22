using WormsRandomizer.Config;
using WormsRandomizer.Random;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer
{
    internal interface ISchemeRandomizer
    {
        IReadOnlyScheme RandomScheme(IRng rng, SchemeRandomizerConfig schemeConfig, WeaponSetConfig weaponSetConfig);
    }
}