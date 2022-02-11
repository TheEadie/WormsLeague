using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes.Random
{
    public class RandomSchemeGenerator : IRandomSchemeGenerator
    {
        private readonly System.Random _rng = new System.Random();

        private static readonly IDictionary<Weapon, byte[]> Powers = new ReadOnlyDictionary<Weapon, byte[]>(
            new Dictionary<Weapon, byte[]>
            {
                {
                    Weapon.Bazooka, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.HomingMissile, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Mortar, new byte[]
                    {
                        10,
                        5,
                        11,
                        18,
                        6,
                        0,
                        1,
                        2,
                        3,
                        4,
                        8,
                        13,
                        9,
                        14
                    }
                },
                {
                    Weapon.Grenade, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.ClusterBomb, new byte[]
                    {
                        10,
                        5,
                        11,
                        18,
                        6,
                        0,
                        1,
                        2,
                        3,
                        4,
                        8,
                        13,
                        9,
                        14
                    }
                },
                {
                    Weapon.Skunk, new byte[]
                    {
                        10,
                        11,
                        0,
                        12,
                        1,
                        18,
                        2,
                        13,
                        8,
                        3,
                        9,
                        4,
                        14
                    }
                },
                {
                    Weapon.PetrolBomb, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.BananaBomb, new byte[]
                    {
                        //255,
                        10,
                        11,
                        18,
                        5,
                        6,
                        0,
                        1,
                        2,
                        3,
                        4,
                        8,
                        13,
                        9,
                        14
                    }
                },
                {
                    Weapon.Handgun, new byte[]
                    {
                        10,
                        0,
                        2,
                        4,
                        13,
                        14
                    }
                },
                {
                    Weapon.Shotgun, new byte[]
                    {
                        10,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Uzi, new byte[]
                    {
                        10,
                        0,
                        2,
                        4,
                        13,
                        14
                    }
                },
                {
                    Weapon.Minigun, new byte[]
                    {
                        10,
                        0,
                        2,
                        4,
                        13,
                        14
                    }
                },
                {
                    Weapon.Longbow, new byte[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4
                    }
                },
                {
                    Weapon.Airstrike, new byte[]
                    {
                        10,
                        5,
                        11,
                        6,
                        0,
                        1,
                        2,
                        3,
                        4,
                        8,
                        13,
                        9,
                        14
                    }
                },
                {
                    Weapon.NapalmStrike, new byte[]
                    {
                        10,
                        5,
                        11,
                        6,
                        0,
                        1,
                        2,
                        3,
                        4,
                        8,
                        13,
                        9,
                        14
                    }
                },
                {
                    Weapon.Mine, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Firepunch, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Dragonball, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Kamikaze, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                { Weapon.Prod, new byte[] { 2 } },
                {
                    Weapon.BattleAxe, new byte[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4
                    }
                },
                {
                    Weapon.Blowtorch, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.PneumaticDrill, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                { Weapon.Girder, new byte[] { 3 } },
                { Weapon.NinjaRope, new byte[] { 4 } },
                { Weapon.Parachute, new byte[] { 0 } },
                { Weapon.Bungee, new byte[] { 0 } },
                { Weapon.Teleport, new byte[] { 0 } },
                {
                    Weapon.Dynamite, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Sheep, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.BaseballBat, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.Flamethrower, new byte[]
                    {
                        20,
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.HomingPigeon, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.MadCow, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.HolyHandGrenade, new byte[]
                    {
                        255,
                        0,
                        1,
                        2,
                        3
                    }
                },
                {
                    Weapon.OldWoman, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.SheepLauncher, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.SuperSheep, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                {
                    Weapon.MoleBomb, new byte[]
                    {
                        10,
                        11,
                        0,
                        1,
                        2,
                        3,
                        4,
                        18,
                        13,
                        14
                    }
                },
                { Weapon.Jetpack, new byte[] { 0 } },
                { Weapon.LowGravity, new byte[] { 0 } },
                { Weapon.LaserSight, new byte[] { 0 } },
                { Weapon.FastWalk, new byte[] { 0 } },
                { Weapon.Invisibility, new byte[] { 0 } },
                { Weapon.DamageX2, new byte[] { 0 } },
                { Weapon.Freeze, new byte[] { 0 } },
                { Weapon.SuperBananaBomb, new byte[] { 0 } },
                { Weapon.MineStrike, new byte[] { 0 } },
                { Weapon.GirderStarterPack, new byte[] { 0 } },
                { Weapon.Earthquake, new byte[] { 0 } },
                { Weapon.ScalesOfJustice, new byte[] { 0 } },
                { Weapon.MingVase, new byte[] { 0 } },
                { Weapon.MikesCarpetBomb, new byte[] { 0 } },
                { Weapon.MagicBullet, new byte[] { 0 } },
                { Weapon.NuclearTest, new byte[] { 0 } },
                { Weapon.SelectWorm, new byte[] { 0 } },
                { Weapon.SalvationArmy, new byte[] { 0 } },
                { Weapon.MoleSquadron, new byte[] { 0 } },
                { Weapon.MBBomb, new byte[] { 0 } },
                { Weapon.ConcreteDonkey, new byte[] { 0 } },
                { Weapon.SuicideBomber, new byte[] { 0 } },
                { Weapon.SheepStrike, new byte[] { 0 } },
                { Weapon.MailStrike, new byte[] { 0 } },
                { Weapon.Armageddon, new byte[] { 0 } },
            });


        public Scheme Generate()
        {
            var scheme = CreateBaseScheme();
            RandomizeWeapons(scheme);
            RandomUtilities(scheme);
            RandomMovementTools(scheme);

            scheme.Weapons[Weapon.NinjaRope].Ammo = 10;

            return scheme;
        }

        private bool IsMovement(Weapon weapon)
        {
            return new[]
            {
                Weapon.NinjaRope,
                Weapon.Parachute,
                Weapon.Bungee
            }.Contains(weapon);
        }

        private bool IsUtility(Weapon weapon)
        {
            return new[]
            {
                Weapon.Prod,
                Weapon.Teleport,
                Weapon.Girder,
                Weapon.Jetpack,
                Weapon.LowGravity,
                Weapon.LaserSight,
                Weapon.FastWalk,
                Weapon.Invisibility,
                Weapon.Freeze,
                Weapon.SelectWorm
            }.Contains(weapon);
        }

        private void RandomizeWeapons(Scheme scheme)
        {
            var allWeapons = Powers.Keys.Where(x => !IsMovement(x) && !IsUtility(x) && !x.IsSuperWeapon()).ToArray();
            var starting = DecideGuaranteedStartingWeapons(allWeapons).ToArray();
            var powerful = DecideGuaranteedPowerfulWeapons(starting).ToArray();

            foreach (var weaponName in allWeapons)
            {
                var ammo = starting.Contains(weaponName) ? (sbyte)1 : (sbyte)0;
                var power = powerful.Contains(weaponName) ? Powers[weaponName].Last() : Powers[weaponName].RandomChoice(_rng);
                var delay = DecideWeaponDelay(power, Powers[weaponName]);

                scheme.Weapons[weaponName].Ammo = ammo;
                scheme.Weapons[weaponName].Power = power;
                scheme.Weapons[weaponName].Delay = delay;
                scheme.Weapons[weaponName].Prob = 1;
            }
        }

        private void RandomUtilities(Scheme scheme)
        {
            var utilitiesToRandomize = Powers.Keys.Where(IsUtility).ToArray();

            foreach (var utilityName in utilitiesToRandomize)
            {
                var ammo = (sbyte)_rng.Next(2);
                var delay = DecideUtilityDelay();
                var power = Powers[utilityName].First();


                scheme.Weapons[utilityName].Ammo = ammo;
                scheme.Weapons[utilityName].Power = power;
                scheme.Weapons[utilityName].Delay = delay;
                scheme.Weapons[utilityName].Prob = 0;
            }
        }

        private void RandomMovementTools(Scheme scheme)
        {
            var movementToolsToRandomize = Powers.Keys.Where(IsMovement).ToArray();

            foreach (var movementName in movementToolsToRandomize)
            {
                var power = Powers[movementName].RandomChoice(_rng);

                scheme.Weapons[movementName].Ammo = 10;
                scheme.Weapons[movementName].Power = power;
                scheme.Weapons[movementName].Delay = 0;
                scheme.Weapons[movementName].Prob = 0;
            }
        }

        private IEnumerable<Weapon> DecideGuaranteedStartingWeapons(Weapon[] configWeapons)
        {
            return configWeapons.Shuffle(_rng)
                .Distinct()
                .Take(4);
        }

        private IEnumerable<Weapon> DecideGuaranteedPowerfulWeapons(IEnumerable<Weapon> startingWeapons)
        {
            return Powers
                .Where(x => x.Value.Length > 1)
                .Select(x => x.Key)
                .Except(startingWeapons)
                .Shuffle(_rng)
                .Take(5);
        }

        private sbyte DecideWeaponDelay(byte powerValue, byte[] powerValues)
        {
            var powerFraction = powerValues.FractionThrough(powerValue);

            if (powerFraction < 0.33d)
            {
                return 0;
            }

            var larger = (int)Math.Floor(powerFraction * 100);
            var smaller = (int)Math.Floor((1 - powerFraction) * 100);

            var sections = new []
            {
                Tuple.Create((sbyte)2, smaller),
                Tuple.Create((sbyte)3, smaller),
                Tuple.Create((sbyte)4, larger),
                Tuple.Create((sbyte)5, larger)
            };
            return sections.RouletteWheel(_rng);
        }
        private sbyte DecideUtilityDelay()
        {
            var sections = new[]
            {
                Tuple.Create((sbyte)1, 1),
                Tuple.Create((sbyte)2, 2),
                Tuple.Create((sbyte)3, 3),
                Tuple.Create((sbyte)4, 2),
                Tuple.Create((sbyte)5, 1)
            };
            return sections.RouletteWheel(_rng);
        }

        private static Scheme CreateBaseScheme()
        {
            var scheme = new Scheme();

            scheme.Version = SchemeVersion.Version3;

            scheme.HotSeatTime = 5;
            scheme.RetreatTime = 3;
            scheme.RetreatTimeRope = 5;
            scheme.ShowRoundTime = true;
            scheme.Replays = false;
            scheme.FallDamage = 1;
            scheme.ArtilleryMode = false;
            scheme.Stockpiling = Stockpiling.Anti;
            scheme.WormSelect = 0;
            scheme.SuddenDeathEvent = SuddenDeathEvent.WaterRise;
            scheme.WaterRiseRate = 20;
            scheme.WeaponCrateProb = 100;
            scheme.HealthCrateProb = 0;
            scheme.UtilityCrateProb = 0;
            scheme.HealthCrateEnergy = 50;
            scheme.DonorCards = true;
            scheme.ObjectTypes = MapObjectType.Both;
            scheme.ObjectCount = 8;
            scheme.MineDelayRandom = false;
            scheme.MineDelay = 3;
            scheme.DudMines = false;
            scheme.ManualWormPlacement = false;
            scheme.WormEnergy = 100;
            scheme.TurnTimeInfinite = false;
            scheme.TurnTime = 45;
            scheme.RoundTimeMinutes = 10;
            scheme.RoundTimeSeconds = 0;
            scheme.NumberOfWins = 1;
            scheme.Blood = false;
            scheme.AquaSheep = false;
            scheme.SheepHeaven = false;
            scheme.GodWorms = false;
            scheme.IndiLand = false;
            scheme.UpgradeGrenade = false;
            scheme.UpgradeShotgun = false;
            scheme.UpgradeCluster = false;
            scheme.UpgradeLongbow = false;
            scheme.TeamWeapons = false;
            scheme.SuperWeapons = true;

            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                (sbyte ammo, byte power, sbyte delay, sbyte prob) = (0, 1, 0, 0);
                scheme.Weapons[weaponName].Ammo = ammo;
                scheme.Weapons[weaponName].Power = power;
                scheme.Weapons[weaponName].Delay = delay;
                scheme.Weapons[weaponName].Prob = prob;
            }

            scheme.Extended.ConstantWind = false;
            scheme.Extended.Wind = 100;
            scheme.Extended.WindBias = 15;
            scheme.Extended.Gravity = 0.23999023f;
            scheme.Extended.Friction = 0.95999146f;
            scheme.Extended.RopeKnockForce = null;
            scheme.Extended.BloodAmount = null;
            scheme.Extended.RopeUpgrade = false;
            scheme.Extended.GroupPlaceAllies = false;
            scheme.Extended.NoCrateProbability = null;
            scheme.Extended.CrateLimit = 5;
            scheme.Extended.SuddenDeathNoWormSelect = true;
            scheme.Extended.SuddenDeathTurnDamage = 5;
            scheme.Extended.WormPhasingAlly = WormPhasing.None;
            scheme.Extended.WormPhasingEnemy = WormPhasing.None;
            scheme.Extended.CircularAim = false;
            scheme.Extended.AntiLockAim = false;
            scheme.Extended.AntiLockPower = false;
            scheme.Extended.WormSelectKeepHotSeat = false;
            scheme.Extended.WormSelectAnytime = false;
            scheme.Extended.BattyRope = false;
            scheme.Extended.RopeRollDrops = RopeRollDrops.None;
            scheme.Extended.KeepControlXImpact = XImpactControlLoss.Loss;
            scheme.Extended.KeepControlHeadBump = false;
            scheme.Extended.KeepControlSkim = SkimControlLoss.Lost;
            scheme.Extended.ExplosionFallDamage = false;
            scheme.Extended.ObjectPushByExplosion = null;
            scheme.Extended.UndeterminedCrates = null;
            scheme.Extended.UndeterminedMineFuse = null;
            scheme.Extended.FiringPausesTimer = true;
            scheme.Extended.LoseControlDoesntEndTurn = false;
            scheme.Extended.ShotDoesntEndTurn = false;
            scheme.Extended.ShotDoesntEndTurnAll = false;
            scheme.Extended.DrillImpartsVelocity = null;
            scheme.Extended.GirderRadiusAssist = false;
            scheme.Extended.FlameTurnDecay = 0.19998169f;
            scheme.Extended.FlameTouchDecay = 30;
            scheme.Extended.FlameLimit = 200;
            scheme.Extended.ProjectileMaxSpeed = 32;
            scheme.Extended.RopeMaxSpeed = 16;
            scheme.Extended.JetpackMaxSpeed = 5;
            scheme.Extended.GameSpeed = 1;
            scheme.Extended.IndianRopeGlitch = null;
            scheme.Extended.HerdDoublingGlitch = null;
            scheme.Extended.JetpackBungeeGlitch = true;
            scheme.Extended.AngleCheatGlitch = true;
            scheme.Extended.GlideGlitch = true;
            scheme.Extended.SkipWalk = SkipWalk.Default;
            scheme.Extended.Roofing = Roofing.Default;
            scheme.Extended.FloatingWeaponGlitch = true;
            scheme.Extended.WormBounce = 0f;
            scheme.Extended.Viscosity = 0f;
            scheme.Extended.ViscosityWorms = false;
            scheme.Extended.RwWind = 0f;
            scheme.Extended.RwWindWorms = false;
            scheme.Extended.RwGravityType = RwGravityType.None;
            scheme.Extended.RwGravity = 1f;
            scheme.Extended.CrateRate = 0;
            scheme.Extended.CrateShower = false;
            scheme.Extended.AntiSink = false;
            scheme.Extended.WeaponsDontChange = false;
            scheme.Extended.ExtendedFuse = false;
            scheme.Extended.AutoReaim = false;
            scheme.Extended.TerrainOverlapGlitch = null;
            scheme.Extended.RoundTimeFractional = false;
            scheme.Extended.AutoRetreat = false;
            scheme.Extended.HealthCure = HealthCure.Team;
            scheme.Extended.KaosMod = 0;
            scheme.Extended.SheepHeavenFlags = SheepHeavenFlags.All;
            scheme.Extended.ConserveUtilities = false;
            scheme.Extended.ExpediteUtilities = false;
            scheme.Extended.DoubleTimeCount = 1;
            return scheme;
        }
    }
}
