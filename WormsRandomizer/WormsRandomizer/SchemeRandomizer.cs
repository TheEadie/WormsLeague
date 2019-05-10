using System;
using System.Collections.Generic;
using System.Linq;
using WormsRandomizer.Config;
using WormsRandomizer.Random;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer
{
    internal class SchemeRandomizer : ISchemeRandomizer
    {
        public IReadOnlyScheme RandomScheme(IRng rng, SchemeRandomizerConfig schemeConfig, WeaponSetConfig weaponSetConfig)
        {
            var weaponSetRandomizer = new WeaponSetRandomizer(rng, schemeConfig.WeaponRandomizerConfig, weaponSetConfig);
            var weapons = weaponSetRandomizer.RandomWeapons();
            var utilities = weaponSetRandomizer.RandomUtilities();
            var movement = weaponSetRandomizer.RandomMovementTools();

            var mineDelay = schemeConfig.RandomPerMine ? 0xFF : rng.Next(4);
            var waterRiseRate = schemeConfig.RandomFloodRate ? GetRandomFloodRate(rng) : 2;

            var scheme = new Scheme
            {
                WeaponInfo = AllWeaponsInOrder(weapons.Concat(utilities).Concat(movement)).ToArray(),
                InitialWormEnergy = schemeConfig.WormHealth,
                SuperWeapons = schemeConfig.AllowSuperWeapons,
                SheepHeaven = schemeConfig.SheepHeaven,
                AquaSheep = schemeConfig.AquaSheep,
                UpgradedGrenade = schemeConfig.UpgradedGrenade,
                UpgradedClusterBombs = schemeConfig.UpgradedClusterBombs,
                UpgradedShotgun = schemeConfig.UpgradedShotgun,
                UpgradedLongbow = schemeConfig.UpgradedLongbow,
                DudMines = schemeConfig.AllowDudMines,
                MineDelay = mineDelay,
                WaterRiseRate = waterRiseRate
            };
            return scheme;
        }

        private IEnumerable<IReadOnlyWeapon> AllWeaponsInOrder(IEnumerable<IReadOnlyWeapon> weapons)
        {
            var weaponsArray = weapons.ToArray();
            foreach (var weaponName in Weapons.AllWeapons)
            {
                var foundWeapon = weaponsArray.FirstOrDefault(x => x.Name == weaponName);
                yield return foundWeapon ?? new Weapon(weaponName, 0, 1, 0, 0);
            }
        }

        private int GetRandomFloodRate(IRng rng)
        {
            var rates = new[]
            {
                Tuple.Create(1, 1),
                Tuple.Create(2, 1),
                Tuple.Create(3, 1),
                Tuple.Create(8, 1),
            };
            return rates.RouletteWheel(rng);
        }
    }
}