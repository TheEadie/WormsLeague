using System.Collections.Generic;
using WormsRandomizer.Config;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer.Flags
{
    internal class FlagParser : IFlagParser
    {
        public void ParseFlag(string arg, SchemeRandomizerConfig config)
        {
            ParseUtils.MatchNumber(arg, "H", n => config.WormHealth = n);
            ParseUtils.MatchBool(arg, "SUPERS", b => config.AllowSuperWeapons = b);
            ParseUtils.MatchBool(arg, "HEAVEN", b => config.SheepHeaven = b);
            ParseUtils.MatchBool(arg, "UG", b => config.UpgradedGrenade = b);
            ParseUtils.MatchBool(arg, "US", b => config.UpgradedShotgun = b);
            ParseUtils.MatchBool(arg, "UL", b => config.UpgradedLongbow = b);
            ParseUtils.MatchBool(arg, "UC", b => config.UpgradedClusterBombs = b);
            ParseUtils.MatchBool(arg, "UA", b => config.AquaSheep = b);
            ParseUtils.MatchBool(arg, "FLOOD", b => config.RandomFloodRate = b);
            ParseUtils.MatchBool(arg, "MINE-DUD", b => config.AllowDudMines = b);
            ParseUtils.MatchBool(arg, "MINE-ANY", b => config.RandomPerMine = b);

            ParseUtils.MatchBool(arg, "OUT-STARTING", b => config.OutputStarting = b);
            ParseUtils.MatchBool(arg, "OUT-SUMMARY", b => config.OutputSummary = b);
            ParseUtils.MatchBool(arg, "OUT-JSON", b => config.OutputJson = b);
            ParseUtils.MatchBool(arg, "OUT-SCHEME", b => config.OutputScheme = b);

            ParseWeaponConfigOptions(arg, config.WeaponRandomizerConfig);
        }

        public IEnumerable<string> GetFlagsHelp()
        {
            var weapons = new WeaponRandomizerConfig();
            yield return "**WEAPON OPTIONS**";
            yield return ParseUtils.DescribeAny("+[weapon]", "[Weapon] will be more common");
            yield return ParseUtils.DescribeAny("-[weapon]", "[Weapon] will never appear");
            yield return ParseUtils.DescribeAny("*[weapon]", "always start with [Weapon]");
            yield return ParseUtils.DescribeBool("d", "powerful weapons are more likely to be delayed for longer. Very weak weapons are never delayed.", weapons.PowerIsDelayed);
            yield return ParseUtils.DescribeNumber("p", "minimum number of powerful weapons", weapons.NumPowerfulWeapons);
            yield return ParseUtils.DescribeNumber("s", "minimum number of starting weapons", weapons.NumStartingWeapons);
            yield return ParseUtils.DescribeNumber("au", "ammo quantity for utility weapons", weapons.UtilityStartingAmmo);
            yield return ParseUtils.DescribeNumber("aw", "ammo quantity for starting weapons", weapons.WeaponStartingAmmo);

            var scheme = new SchemeRandomizerConfig();
            yield return "**GAME OPTIONS**";
            yield return ParseUtils.DescribeNumber("h", "worm starting health", scheme.WormHealth);
            yield return ParseUtils.DescribeBool("super", "super weapons are enabled", scheme.AllowSuperWeapons);
            yield return ParseUtils.DescribeBool("heaven", "sheep heaven - exploding crates contain sheep", scheme.SheepHeaven);
            yield return ParseUtils.DescribeBool("ug", "upgraded grenade - grenades are more powerful", scheme.UpgradedGrenade);
            yield return ParseUtils.DescribeBool("us", "upgraded shotgun - shotgun shoots extra shots", scheme.UpgradedShotgun);
            yield return ParseUtils.DescribeBool("ul", "upgraded longbow - longbows are more powerful", scheme.UpgradedLongbow);
            yield return ParseUtils.DescribeBool("uc", "upgraded cluster bombs - cluster bombs contain more clusters", scheme.UpgradedClusterBombs);
            yield return ParseUtils.DescribeBool("ua", "upgraded sheep - super sheep are aqua sheep", scheme.AquaSheep);
            yield return ParseUtils.DescribeBool("flood", "randomise the sudden death water rise rate", scheme.RandomFloodRate);
            yield return ParseUtils.DescribeBool("mine-dud", "mines can be duds", scheme.AllowDudMines);
            yield return ParseUtils.DescribeBool("mine-any", "If enabled each mine will have a random delay rather than all mines having the same (random) delay", scheme.AllowDudMines);

            yield return "**OUTPUT OPTIONS**";
            yield return ParseUtils.DescribeBool("out-starting", "Output a list of starting weapons to the console", scheme.OutputStarting);
            yield return ParseUtils.DescribeBool("out-summary", "Output a randomised stats of all available weapons to the console", scheme.OutputSummary);
            yield return ParseUtils.DescribeBool("out-json", "Output the scheme as a json file", scheme.OutputJson);
            yield return ParseUtils.DescribeBool("out-scheme", "Output the scheme as a wsc file", scheme.OutputScheme);
            yield return string.Empty;
        }

        private static void ParseWeaponConfigOptions(string arg, WeaponRandomizerConfig config)
        {
            ParseUtils.MatchAnyBool(arg, string.Empty, Weapons.AllWeapons, (w,p) => PromoteOrBanWeapon(w, p, config));
            ParseUtils.MatchAny(arg, "*", Weapons.AllWeapons, w => config.RequiredStartingWeapons.Add(w));
            ParseUtils.MatchBool(arg, "D", b => config.PowerIsDelayed = b);
            ParseUtils.MatchNumber(arg, "P", n => config.NumPowerfulWeapons = n);
            ParseUtils.MatchNumber(arg, "S", n => config.NumStartingWeapons = n);
            ParseUtils.MatchNumber(arg, "AU", n => config.UtilityStartingAmmo = n);
            ParseUtils.MatchNumber(arg, "AW", n => config.WeaponStartingAmmo = n);
        }

        private static void PromoteOrBanWeapon(string weapon, bool promoteWeapon, WeaponRandomizerConfig config)
        {
            if (promoteWeapon)
            {
                config.PromotedWeapons.Add(weapon);
            }
            else
            {
                config.BannedWeapons.Add(weapon);
            }
        }
    }
}