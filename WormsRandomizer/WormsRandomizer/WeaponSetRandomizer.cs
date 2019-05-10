using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using WormsRandomizer.Config;
using WormsRandomizer.Random;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer
{
    internal class WeaponSetRandomizer
    {
        private readonly IRng _rng;
        private readonly WeaponRandomizerConfig _randomizerConfig;
        private readonly WeaponSetConfig _setConfig;

        public WeaponSetRandomizer(IRng rng, WeaponRandomizerConfig randomizerConfig, WeaponSetConfig setConfig)
        {
            _rng = rng;
            _randomizerConfig = randomizerConfig;
            _setConfig = setConfig;
        }

        public IEnumerable<IReadOnlyWeapon> RandomWeapons()
        {
            var weaponsToRandomize = _setConfig.Weapons
                .Where(x => !_setConfig.SuperWeapons.Contains(x.Name))
                .Where(x => !_setConfig.UtilityWeapons.Contains(x.Name))
                .Where(x => !_setConfig.MovementWeapons.Contains(x.Name))
                .Where(x => !_randomizerConfig.BannedWeapons.Contains(x.Name))
                .ToArray();

            var starting = DecideGuaranteedStartingWeapons(weaponsToRandomize).ToArray();
            var powerful = DecideGuaranteedPowerfulWeapons(weaponsToRandomize, starting).ToArray();

            foreach (var config in weaponsToRandomize)
            {
                var ammo = starting.Contains(config.Name) ? _randomizerConfig.WeaponStartingAmmo : 0;
                var power = powerful.Contains(config.Name) ? config.Power.Last() : config.Power.RandomChoice(_rng);
                var delay = DecideWeaponDelay(power, config.Power);
                var crateChance = _randomizerConfig.PromotedWeapons.Count(x=> x == config.Name) + 1;

                yield return new Weapon(config.Name, ammo, power, delay, crateChance);
            }
        }

        public IEnumerable<IReadOnlyWeapon> RandomUtilities()
        {
            var utilitiesToRandomize = _setConfig.Weapons
                .Where(x => _setConfig.UtilityWeapons.Contains(x.Name))
                .Where(x => !_randomizerConfig.BannedWeapons.Contains(x.Name))
                .ToArray();

            foreach (var config in utilitiesToRandomize)
            {
                var ammo = _randomizerConfig.UtilityStartingAmmo * _rng.Next(2);
                var delay = DecideUtilityDelay();
                var power = config.Power.First();

                yield return new Weapon(config.Name, ammo, power, delay, 0);
            }
        }

        public IEnumerable<IReadOnlyWeapon> RandomMovementTools()
        {
            var movementToolsToRandomize = _setConfig.Weapons
                .Where(x => _setConfig.MovementWeapons.Contains(x.Name))
                .Where(x => !_randomizerConfig.BannedWeapons.Contains(x.Name))
                .ToArray();

            foreach (var config in movementToolsToRandomize)
            {
                var power = config.Power.RandomChoice(_rng);

                yield return new Weapon(config.Name, 10, power, 0, 0);
            }
        }

        private IEnumerable<string> DecideGuaranteedStartingWeapons(IEnumerable<WeaponConfig> configWeapons)
        {
            return _randomizerConfig.RequiredStartingWeapons
                .Concat(configWeapons.Select(x => x.Name).Shuffle(_rng))
                .Distinct()
                .Take(_randomizerConfig.NumStartingWeapons);
        }

        private IEnumerable<string> DecideGuaranteedPowerfulWeapons(IEnumerable<WeaponConfig> configWeapons, IEnumerable<string> startingWeapons)
        {
            return configWeapons
                .Where(x => x.Power.Count > 1)
                .Select(x => x.Name)
                .Except(startingWeapons)
                .Shuffle(_rng)
                .Take(_randomizerConfig.NumPowerfulWeapons);
        }

        private int DecideWeaponDelay(int powerValue, IReadOnlyList<int> powerValues)
        {
            var powerFraction = powerValues.FractionThrough(powerValue);

            if (!_randomizerConfig.PowerIsDelayed || powerFraction < 0.33d)
            {
                return 0;
            }

            var larger = (int)Math.Floor(powerFraction * 100);
            var smaller = (int)Math.Floor((1 - powerFraction) * 100);

            var sections = new[]
            {
                Tuple.Create(2, smaller),
                Tuple.Create(3, smaller),
                Tuple.Create(4, larger),
                Tuple.Create(5, larger)
            };
            return sections.RouletteWheel(_rng);
        }

        private int DecideUtilityDelay()
        {
            var sections = new[]
            {
                Tuple.Create(1, 1),
                Tuple.Create(2, 2),
                Tuple.Create(3, 3),
                Tuple.Create(4, 2),
                Tuple.Create(5, 1)
            };
            return sections.RouletteWheel(_rng);
        }
    }
}
