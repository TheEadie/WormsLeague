using System;
using System.IO;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes.Text
{
    internal class SchemeTextWriter : ISchemeTextWriter
    {
        public void Write(Scheme definition, TextWriter textWriter)
        {
            WriteHeader(textWriter, "GENERAL");
            WriteItem(textWriter, "Hot seat delay", definition.HotSeatTime, "Seconds");
            WriteItem(textWriter, "Retreat time", definition.RetreatTime, "Seconds");
            WriteItem(textWriter, "Rope retreat time", definition.RetreatTimeRope, "Seconds");
            WriteItem(textWriter, "Display total round time", definition.ShowRoundTime);
            WriteItem(textWriter, "Automatic replays", definition.Replays);
            WriteItem(textWriter, "Fall damage", definition.FallDamage);
            WriteItem(textWriter, "Artillery mode", definition.ArtilleryMode, "Worms can't move");
            WriteItem(
                textWriter,
                "Stockpiling mode",
                definition.Stockpiling,
                "Off | On | Anti");
            WriteItem(textWriter, "Worms select", definition.WormSelect, "Sequential | Manual | Random");
            WriteItem(
                textWriter,
                "Sudden death event",
                definition.SuddenDeathEvent,
                "RoundEnd | NuclearStrike | HealthDrop | WaterRise");
            WriteItem(
                textWriter,
                "Sudden death water rise rate",
                definition.WaterRiseRate,
                "See table on http://worms2d.info/Sudden_Death");
            WriteItem(
                textWriter,
                "Weapon crate probability",
                definition.WeaponCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                textWriter,
                "Health crate probability",
                definition.HealthCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                textWriter,
                "Utility crate probability",
                definition.UtilityCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(textWriter, "Health crate energy", definition.HealthCrateEnergy);
            WriteItem(textWriter, "Donor cards", definition.DonorCards);
            WriteItem(textWriter, "Hazard objects", definition.ObjectTypes, "None | Mines | OilDrums | Both");
            WriteItem(textWriter, "Max num of hazard objects", definition.ObjectCount);
            WriteItem(textWriter, "Random mine delay", definition.MineDelayRandom, "If set mine delay will be ignored");
            WriteItem(textWriter, "Mine delay", definition.MineDelay);
            WriteItem(textWriter, "Dud mines", definition.DudMines);
            WriteItem(textWriter, "Worm placement", definition.ManualWormPlacement);
            WriteItem(textWriter, "Initial worm energy", definition.WormEnergy);
            WriteItem(textWriter, "Infinite turn time", definition.TurnTimeInfinite, "If set turn time will be ignored");
            WriteItem(textWriter, "Turn time", definition.TurnTime, "Seconds");
            WriteItem(textWriter, "Round time (mins)", definition.RoundTimeMinutes, "Minutes");
            WriteItem(textWriter, "Round time (secs)", definition.RoundTimeSeconds, "Seconds");
            WriteItem(textWriter, "Number of rounds", definition.NumberOfWins);
            WriteItem(textWriter, "Blood", definition.Blood);
            WriteItem(textWriter, "Aqua sheep", definition.AquaSheep);
            WriteItem(
                textWriter,
                "Sheep heaven",
                definition.SheepHeaven,
                "Exploding sheep jump out of destroyed weapon crates");
            WriteItem(textWriter, "God worms", definition.GodWorms, "Worms can't lose health");
            WriteItem(textWriter, "Indestructible land", definition.IndiLand);
            WriteItem(textWriter, "Upgraded grenade", definition.UpgradeGrenade);
            WriteItem(textWriter, "Upgraded shotgun", definition.UpgradeShotgun);
            WriteItem(textWriter, "Upgraded cluster bombs", definition.UpgradeCluster);
            WriteItem(textWriter, "Upgraded longbow", definition.UpgradeLongbow);
            WriteItem(
                textWriter,
                "Team weapons",
                definition.TeamWeapons,
                "Teams will start the match with their preselected team weapon");
            WriteItem(textWriter, "Super weapons", definition.SuperWeapons, "Super weapons may appear in crates");
            textWriter.WriteLine();

            WriteHeader(textWriter, "WEAPONS");
            textWriter.WriteLine("(See http://worms2d.info/Weapons for what various power settings will do)");
            textWriter.WriteLine();

            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                var weapon = definition.Weapons[weaponName];
                var ammoPadding = weapon.Ammo > 9 ? "   " : "    ";
                var powerPadding = weapon.Power > 9 ? "   " : "    ";
                var delayPadding = weapon.Delay > 9 ? "   " : "    ";
                textWriter.WriteLine(
                    weaponName.ToString().PadRight(30)
                    + "Ammo: ["
                    + weapon.Ammo
                    + "]"
                    + ammoPadding
                    + "Power: ["
                    + weapon.Power
                    + "]"
                    + powerPadding
                    + "Delay: ["
                    + weapon.Delay
                    + "]"
                    + delayPadding
                    + "Crate probability: ["
                    + weapon.Prob
                    + "]");
            }

            textWriter.WriteLine();
            WriteHeader(textWriter, "EXTENDED OPTIONS");
            textWriter.WriteLine("(See https://worms2d.info/Game_scheme_file for docs)");
            textWriter.WriteLine();

            WriteItem(textWriter, "Constant Wind", definition.Extended.ConstantWind);
            WriteItem(textWriter, "Wind", definition.Extended.Wind);
            WriteItem(textWriter, "Wind Bias", definition.Extended.WindBias);
            WriteItem(textWriter, "Gravity", definition.Extended.Gravity);
            WriteItem(textWriter, "Terrain Friction", definition.Extended.Friction);
            WriteItem(textWriter, "Rope Knocking", definition.Extended.RopeKnockForce);
            WriteItem(textWriter, "Blood Level", definition.Extended.BloodAmount);
            WriteItem(textWriter, "Unrestrict Rope", definition.Extended.RopeUpgrade);
            WriteItem(textWriter, "Auto-Place Worms by Ally", definition.Extended.GroupPlaceAllies);
            WriteItem(textWriter, "No-Crate Probability", definition.Extended.NoCrateProbability);
            WriteItem(textWriter, "Maximum Crate Count on Map", definition.Extended.CrateLimit);
            WriteItem(textWriter, "Sudden Death Disables Worm Select", definition.Extended.SuddenDeathNoWormSelect);
            WriteItem(textWriter, "Sudden Death Worm Damage Per Turn", definition.Extended.SuddenDeathTurnDamage);
            WriteItem(textWriter, "Phased Worms (Allied)", definition.Extended.WormPhasingAlly, "None | Worms | WormsWeapons | WormsWeaponsDamage");
            WriteItem(textWriter, "Phased Worms (Enemy)", definition.Extended.WormPhasingEnemy, "None | Worms | WormsWeapons | WormsWeaponsDamage");
            WriteItem(textWriter, "Circular Aim", definition.Extended.CircularAim);
            WriteItem(textWriter, "Anti-Lock Aim", definition.Extended.AntiLockAim);
            WriteItem(textWriter, "Anti-Lock Power", definition.Extended.AntiLockPower);
            WriteItem(textWriter, "Worm Selection Doesn't End Hot Seat", definition.Extended.WormSelectKeepHotSeat);
            WriteItem(textWriter, "Worm Selection is Never Cancelled", definition.Extended.WormSelectAnytime);
            WriteItem(textWriter, "Batty Rope", definition.Extended.BattyRope);
            WriteItem(textWriter, "Rope-Roll Drops", definition.Extended.RopeRollDrops, "None | Rope | RopeJump");
            WriteItem(textWriter, "X-Impact Loss of Control", definition.Extended.KeepControlXImpact, "Loss | Kept");
            WriteItem(textWriter, "Keep Control After Bumping Head", definition.Extended.KeepControlHeadBump);
            WriteItem(textWriter, "Keep Control After Skimming", definition.Extended.KeepControlSkim, "Lost | Kept | KeptWithRope");
            WriteItem(textWriter, "Explosions Cause Fall Damage ", definition.Extended.ExplosionFallDamage);
            WriteItem(textWriter, "Explosions Push All Objects", definition.Extended.ObjectPushByExplosion);
            WriteItem(textWriter, "Undetermined Crates", definition.Extended.UndeterminedCrates);
            WriteItem(textWriter, "Undetermined Fuses", definition.Extended.UndeterminedMineFuse);
            WriteItem(textWriter, "Pause Timer While Firing", definition.Extended.FiringPausesTimer);
            WriteItem(textWriter, "Loss of Control Doesn't End Turn", definition.Extended.LoseControlDoesntEndTurn);
            WriteItem(textWriter, "Weapon Use Doesn't End Turn", definition.Extended.ShotDoesntEndTurn);
            WriteItem(textWriter, "Above option doesn't block any weapons", definition.Extended.ShotDoesntEndTurnAll);
            WriteItem(textWriter, "Pneumatic Drill Imparts Velocity", definition.Extended.DrillImpartsVelocity);
            WriteItem(textWriter, "Girder Radius Assist", definition.Extended.GirderRadiusAssist);
            WriteItem(textWriter, "Petrol Turn Decay", definition.Extended.FlameTurnDecay);
            WriteItem(textWriter, "Petrol Touch Decay", definition.Extended.FlameTouchDecay);
            WriteItem(textWriter, "Maximum Flamelet Count", definition.Extended.FlameLimit);
            WriteItem(textWriter, "Maximum Projectile Speed", definition.Extended.ProjectileMaxSpeed);
            WriteItem(textWriter, "Maximum Rope Speed", definition.Extended.RopeMaxSpeed);
            WriteItem(textWriter, "Maximum Jet Pack Speed", definition.Extended.JetpackMaxSpeed);
            WriteItem(textWriter, "Game Engine Speed", definition.Extended.GameSpeed);
            WriteItem(textWriter, "Indian Rope Glitch", definition.Extended.IndianRopeGlitch);
            WriteItem(textWriter, "Herd-Doubling Glitch", definition.Extended.HerdDoublingGlitch);
            WriteItem(textWriter, "Jet Pack Bungee Glitch", definition.Extended.JetpackBungeeGlitch);
            WriteItem(textWriter, "Angle Cheat Glitch", definition.Extended.AngleCheatGlitch);
            WriteItem(textWriter, "Glide Glitch", definition.Extended.GlideGlitch);
            WriteItem(textWriter, "Skipwalking", definition.Extended.SkipWalk, "Default | Facilitated | Disabled");
            WriteItem(textWriter, "Block Roofing", definition.Extended.Roofing, "Default | Up | UpSide");
            WriteItem(textWriter, "Floating Weapon Glitch", definition.Extended.FloatingWeaponGlitch);
            WriteItem(textWriter, "RubberWorm Bounciness", definition.Extended.WormBounce);
            WriteItem(textWriter, "RubberWorm Air Viscosity", definition.Extended.Viscosity);
            WriteItem(textWriter, "RubberWorm Air Viscosity to Worms", definition.Extended.ViscosityWorms);
            WriteItem(textWriter, "RubberWorm Wind Influence", definition.Extended.RwWind);
            WriteItem(textWriter, "RubberWorm Wind Influence to Worms", definition.Extended.RwWindWorms);
            WriteItem(textWriter, "RubberWorm Gravity Type", definition.Extended.RwGravityType, "None | Default | BlackHoleConstant | BlackHoleLinear");
            WriteItem(textWriter, "RubberWorm Gravity Strength", definition.Extended.RwGravity);
            WriteItem(textWriter, "RubberWorm Crate Rate", definition.Extended.CrateRate);
            WriteItem(textWriter, "RubberWorm Crate Shower", definition.Extended.CrateShower);
            WriteItem(textWriter, "RubberWorm Anti-Sink", definition.Extended.AntiSink);
            WriteItem(textWriter, "RubberWorm Remember Weapons", definition.Extended.WeaponsDontChange);
            WriteItem(textWriter, "RubberWorm Extended Fuses/Herds", definition.Extended.ExtendedFuse);
            WriteItem(textWriter, "RubberWorm Anti-Lock Aim", definition.Extended.AutoReaim);
            WriteItem(textWriter, "Terrain Overlap Phasing Glitch", definition.Extended.TerrainOverlapGlitch);
            WriteItem(textWriter, "Fractional Round Timer", definition.Extended.RoundTimeFractional);
            WriteItem(textWriter, "Automatic End-of-Turn Retreat", definition.Extended.AutoRetreat);
            WriteItem(textWriter, "Health Crates Cure Poison", definition.Extended.HealthCure, "Single | Team | Allies | Nobody");
            WriteItem(textWriter, "RubberWorm Kaos Mod", definition.Extended.KaosMod);
            WriteItem(textWriter, "Sheep Heaven's Gate", definition.Extended.SheepHeavenFlags);
            WriteItem(textWriter, "Conserve Instant Utilities", definition.Extended.ConserveUtilities);
            WriteItem(textWriter, "Expedite Instant Utilities", definition.Extended.ExpediteUtilities);
            WriteItem(textWriter, "Double Time Stack Limit", definition.Extended.DoubleTimeCount);

        }

        private static void WriteItem(TextWriter writer, string description, object value, string comment = null)
        {
            var output = $"{description}: ".PadRight(40) + "[" + value + "]";

            if (comment != null)
            {
                output += $" ({comment})";
            }

            writer.WriteLine(output);
        }

        private static void WriteHeader(TextWriter writer, string heading)
        {
            writer.WriteLine("//////////////////////");
            writer.WriteLine($"// {heading}".PadRight(20) + "//");
            writer.WriteLine("//////////////////////");
            writer.WriteLine("");
        }
    }
}
