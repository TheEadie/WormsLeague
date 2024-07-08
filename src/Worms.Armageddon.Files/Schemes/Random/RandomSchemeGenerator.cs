using System.Diagnostics.CodeAnalysis;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Random;

[SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Not used for security")]
public class RandomSchemeGenerator : IRandomSchemeGenerator
{
    public Scheme Generate(int? seed = null)
    {
        var rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random(DateTime.Now.Millisecond);

        var scheme = CreateBaseScheme();
        RandomizeWeapons(scheme, rng);
        RandomUtilities(scheme, rng);
        RandomMovementTools(scheme, rng);

        scheme.Weapons[Weapon.NinjaRope].Ammo = 10;

        return scheme;
    }

    private static void RandomizeWeapons(Scheme scheme, System.Random rng)
    {
        var allWeapons = Enum.GetValues<Weapon>().Where(x => x.IsRegularWeapon()).ToArray();
        var starting = DecideGuaranteedStartingWeapons(allWeapons, rng).ToArray();
        var powerful = DecideGuaranteedPowerfulWeapons(starting, rng).ToArray();

        foreach (var weaponName in allWeapons)
        {
            var ammo = starting.Contains(weaponName) ? (sbyte) 1 : (sbyte) 0;
            var power = powerful.Contains(weaponName)
                ? WeaponUtils.ValidPowerSettings[weaponName].Last()
                : WeaponUtils.ValidPowerSettings[weaponName].RandomChoice(rng);
            var delay = DecideWeaponDelay(power, WeaponUtils.ValidPowerSettings[weaponName], rng);

            scheme.Weapons[weaponName].Ammo = ammo;
            scheme.Weapons[weaponName].Power = power;
            scheme.Weapons[weaponName].Delay = delay;
            scheme.Weapons[weaponName].Prob = 1;
        }
    }

    private static void RandomUtilities(Scheme scheme, System.Random rng)
    {
        foreach (var utilityName in Enum.GetValues<Weapon>().Where(x => x.IsUtility()).ToArray())
        {
            var ammo = (sbyte) rng.Next(2);
            var delay = DecideUtilityDelay(rng);
            var power = WeaponUtils.ValidPowerSettings[utilityName].First();

            scheme.Weapons[utilityName].Ammo = ammo;
            scheme.Weapons[utilityName].Power = power;
            scheme.Weapons[utilityName].Delay = delay;
            scheme.Weapons[utilityName].Prob = 0;
        }
    }

    private static void RandomMovementTools(Scheme scheme, System.Random rng)
    {
        foreach (var movementName in Enum.GetValues<Weapon>().Where(x => x.IsMovement()).ToArray())
        {
            var power = WeaponUtils.ValidPowerSettings[movementName].RandomChoice(rng);

            scheme.Weapons[movementName].Ammo = 10;
            scheme.Weapons[movementName].Power = power;
            scheme.Weapons[movementName].Delay = 0;
            scheme.Weapons[movementName].Prob = 0;
        }
    }

    private static IEnumerable<Weapon> DecideGuaranteedStartingWeapons(Weapon[] configWeapons, System.Random rng) =>
        configWeapons.Shuffle(rng).Distinct().Take(4);

    private static IEnumerable<Weapon> DecideGuaranteedPowerfulWeapons(
        IEnumerable<Weapon> startingWeapons,
        System.Random rng) =>
        WeaponUtils.ValidPowerSettings.Where(x => x.Value.Length > 1)
            .Select(x => x.Key)
            .Except(startingWeapons)
            .Shuffle(rng)
            .Take(5);

    private static sbyte DecideWeaponDelay(byte powerValue, byte[] powerValues, System.Random rng)
    {
        var powerFraction = powerValues.FractionThrough(powerValue);

        if (powerFraction < 0.33d)
        {
            return 0;
        }

        var larger = (int) Math.Floor(powerFraction * 100);
        var smaller = (int) Math.Floor((1 - powerFraction) * 100);

        var sections = new[]
        {
            Tuple.Create((sbyte) 2, smaller),
            Tuple.Create((sbyte) 3, smaller),
            Tuple.Create((sbyte) 4, larger),
            Tuple.Create((sbyte) 5, larger)
        };
        return sections.RouletteWheel(rng);
    }

    private static sbyte DecideUtilityDelay(System.Random rng)
    {
        var sections = new[]
        {
            Tuple.Create((sbyte) 1, 1),
            Tuple.Create((sbyte) 2, 2),
            Tuple.Create((sbyte) 3, 3),
            Tuple.Create((sbyte) 4, 2),
            Tuple.Create((sbyte) 5, 1)
        };
        return sections.RouletteWheel(rng);
    }

    private static Scheme CreateBaseScheme()
    {
        var scheme = new Scheme
        {
            Version = SchemeVersion.Version3,
            HotSeatTime = 5,
            RetreatTime = 3,
            RetreatTimeRope = 5,
            ShowRoundTime = true,
            Replays = false,
            FallDamage = 1,
            ArtilleryMode = false,
            Stockpiling = Stockpiling.Anti,
            WormSelect = 0,
            SuddenDeathEvent = SuddenDeathEvent.WaterRise,
            WaterRiseRate = 20,
            WeaponCrateProb = 100,
            HealthCrateProb = 0,
            UtilityCrateProb = 0,
            HealthCrateEnergy = 50,
            DonorCards = true,
            ObjectTypes = MapObjectType.Both,
            ObjectCount = 8,
            MineDelayRandom = false,
            MineDelay = 3,
            DudMines = false,
            ManualWormPlacement = false,
            WormEnergy = 100,
            TurnTimeInfinite = false,
            TurnTime = 45,
            RoundTimeMinutes = 10,
            RoundTimeSeconds = 0,
            NumberOfWins = 1,
            Blood = false,
            AquaSheep = false,
            SheepHeaven = false,
            GodWorms = false,
            IndiLand = false,
            UpgradeGrenade = false,
            UpgradeShotgun = false,
            UpgradeCluster = false,
            UpgradeLongbow = false,
            TeamWeapons = false,
            SuperWeapons = true
        };

        foreach (var weaponName in Enum.GetValues<Weapon>())
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
