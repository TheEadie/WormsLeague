using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WormsRandomizer.Config;
using WormsRandomizer.Flags;
using WormsRandomizer.Random;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer
{
    internal class RandomizerApp : IRandomizerApp
    {
        private readonly ISchemeRandomizer _schemeRandomizer;
        private readonly IFlagParser _flagParser;
        private readonly IWeaponSetConfigReader _weaponSetConfigReader;
        private readonly IWscWriter _wscWriter;
        private readonly Func<string, IRng> _rngFactory;

        private Version RandomizerVersion => new Version(1, 0);

        public RandomizerApp(
            ISchemeRandomizer schemeRandomizer,
            IFlagParser flagParser,
            IWeaponSetConfigReader weaponSetConfigReader,
            IWscWriter wscWriter,
            Func<string, IRng> rngFactory)
        {
            _schemeRandomizer = schemeRandomizer;
            _flagParser = flagParser;
            _weaponSetConfigReader = weaponSetConfigReader;
            _wscWriter = wscWriter;
            _rngFactory = rngFactory;
        }

        public void DoRandomizer(string[] args)
        {
            var schemeConfig = new SchemeRandomizerConfig();
            var weaponSetConfig = _weaponSetConfigReader.ReadConfig();

            var seed = args[0];
            foreach (var arg in args.Skip(1))
            {
                _flagParser.ParseFlag(arg, schemeConfig);
            }

            var rng = _rngFactory(seed);
            var scheme = _schemeRandomizer.RandomScheme(rng, schemeConfig, weaponSetConfig);

            PrintStartingWeapons(scheme, schemeConfig);
            PrintSummary(scheme, schemeConfig, weaponSetConfig);
            OutputJson(scheme, schemeConfig, seed);
            OutputScheme(scheme, schemeConfig, seed);
        }

        public void PrintHelp()
        {
            Console.WriteLine($"Worms scheme randomizer {RandomizerVersion}");
            Console.WriteLine("wrand -h: Print help");
            Console.WriteLine("wrand -weapons: Print full list of weapons");
            Console.WriteLine("wrand [seed] [flags]");

            foreach (var line in _flagParser.GetFlagsHelp())
            {
                Console.WriteLine("  " + line);
            }
        }

        public void PrintWeaponList()
        {
            foreach (var x in Weapons.AllWeapons.OrderBy(x => x))
            {
                Console.WriteLine(x);
            }
        }

        private void OutputScheme(IReadOnlyScheme scheme, SchemeRandomizerConfig schemeConfig, string seed)
        {
            if (!schemeConfig.OutputScheme)
            {
                return;
            }

            _wscWriter.Write(scheme, $"RandomShop.{seed}.wsc");
            Console.WriteLine($"RandomShop.{seed}.wsc");
        }

        private static void OutputJson(IReadOnlyScheme scheme, SchemeRandomizerConfig schemeConfig, string seed)
        {
            if (!schemeConfig.OutputJson)
            {
                return;
            }

            var schemeJson = JsonConvert.SerializeObject(scheme, Formatting.Indented);
            File.WriteAllText($"RandomShop.{seed}.json", schemeJson);
            Console.WriteLine($"RandomShop.{seed}.wsc");
        }

        private static void PrintStartingWeapons(IReadOnlyScheme scheme, SchemeRandomizerConfig schemeConfig)
        {
            if (!schemeConfig.OutputStarting)
            {
                return;
            }

            var startingWeapons = scheme.WeaponInfo.Where(w => w.Ammo > 0).Select(w => w.Name);
            Console.Write(" Starting weapons:");
            Console.WriteLine(string.Join(",", startingWeapons));
        }

        private static void PrintSummary(IReadOnlyScheme scheme,  SchemeRandomizerConfig schemeConfig, WeaponSetConfig weaponSetConfig)
        {
            if (!schemeConfig.OutputSummary)
            {
                return;
            }

            var weaponSummary = scheme.WeaponInfo
                .Where(w => w.CrateProbability != 0 || w.Ammo != 0)
                .Select(w =>
                {
                    var config = weaponSetConfig.Weapons.First(x => x.Name == w.Name);
                    var fraction = config.Power.FractionThrough(w.Power);
                    return Tuple.Create(w.Name, fraction, w.Delay, w.Ammo, config.Power.Count > 1 ? 1 : 0);
                })
                .OrderByDescending(t => t.Item5)
                .ThenBy(t => t.Item2);

            Console.WriteLine("{0, -20}{1, 10}{2, 10}{3, 10}", "Weapon", "Power", "Delay", "Ammo");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var t in weaponSummary)
            {
                Console.WriteLine("{0, -20}{1, 10:P0}{2, 10}{3, 10}", t.Item1, t.Item2, t.Item3, t.Item4);
            }
        }
    }
}