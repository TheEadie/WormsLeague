using System.Globalization;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Text;

internal class SchemeTextReader : ISchemeTextReader
{
    public Scheme GetModel(string definition)
    {
        var scheme = new Scheme();
        using var b = new StringReader(definition);

        scheme.Version = SchemeVersion.Version3;

        // Skip over some heading lines
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);

        scheme.HotSeatTime = GetByte(b);
        scheme.RetreatTime = GetByte(b);
        scheme.RetreatTimeRope = GetByte(b);
        scheme.ShowRoundTime = GetBool(b);
        scheme.Replays = GetBool(b);
        scheme.FallDamage = GetByte(b);
        scheme.ArtilleryMode = GetBool(b);
        scheme.Stockpiling = GetEnum<Stockpiling>(b);
        scheme.WormSelect = GetEnum<WormSelect>(b);
        scheme.SuddenDeathEvent = GetEnum<SuddenDeathEvent>(b);
        scheme.WaterRiseRate = GetByte(b);
        scheme.WeaponCrateProb = GetSbyte(b);
        scheme.HealthCrateProb = GetSbyte(b);
        scheme.UtilityCrateProb = GetSbyte(b);
        scheme.HealthCrateEnergy = GetByte(b);
        scheme.DonorCards = GetBool(b);
        scheme.ObjectTypes = GetEnum<MapObjectType>(b);
        scheme.ObjectCount = GetByte(b);
        scheme.MineDelayRandom = GetBool(b);
        scheme.MineDelay = GetByte(b);
        scheme.DudMines = GetBool(b);
        scheme.ManualWormPlacement = GetBool(b);
        scheme.WormEnergy = GetByte(b);
        scheme.TurnTimeInfinite = GetBool(b);
        scheme.TurnTime = GetByte(b);
        scheme.RoundTimeMinutes = GetByte(b);
        scheme.RoundTimeSeconds = GetByte(b);
        scheme.NumberOfWins = GetByte(b);
        scheme.Blood = GetBool(b);
        scheme.AquaSheep = GetBool(b);
        scheme.SheepHeaven = GetBool(b);
        scheme.GodWorms = GetBool(b);
        scheme.IndiLand = GetBool(b);
        scheme.UpgradeGrenade = GetBool(b);
        scheme.UpgradeShotgun = GetBool(b);
        scheme.UpgradeCluster = GetBool(b);
        scheme.UpgradeLongbow = GetBool(b);
        scheme.TeamWeapons = GetBool(b);
        scheme.SuperWeapons = GetBool(b);

        // Skip over the middle heading
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);

        foreach (var weaponName in Enum.GetValues<Weapon>())
        {
            var (ammo, power, delay, prob) = GetWeaponDetails(b);
            scheme.Weapons[weaponName].Ammo = ammo;
            scheme.Weapons[weaponName].Power = power;
            scheme.Weapons[weaponName].Delay = delay;
            scheme.Weapons[weaponName].Prob = prob;
        }

        // Skip over the heading
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);
        _ = ReadNextLine(b);

        scheme.Extended.ConstantWind = GetBool(b);
        scheme.Extended.Wind = GetSbyte(b);
        scheme.Extended.WindBias = GetByte(b);
        scheme.Extended.Gravity = GetFloat(b);
        scheme.Extended.Friction = GetFloat(b);
        scheme.Extended.RopeKnockForce = GetNullableByte(b);
        scheme.Extended.BloodAmount = GetNullableByte(b);
        scheme.Extended.RopeUpgrade = GetBool(b);
        scheme.Extended.GroupPlaceAllies = GetBool(b);
        scheme.Extended.NoCrateProbability = GetNullableByte(b);
        scheme.Extended.CrateLimit = GetByte(b);
        scheme.Extended.SuddenDeathNoWormSelect = GetBool(b);
        scheme.Extended.SuddenDeathTurnDamage = GetByte(b);
        scheme.Extended.WormPhasingAlly = GetEnum<WormPhasing>(b);
        scheme.Extended.WormPhasingEnemy = GetEnum<WormPhasing>(b);
        scheme.Extended.CircularAim = GetBool(b);
        scheme.Extended.AntiLockAim = GetBool(b);
        scheme.Extended.AntiLockPower = GetBool(b);
        scheme.Extended.WormSelectKeepHotSeat = GetBool(b);
        scheme.Extended.WormSelectAnytime = GetBool(b);
        scheme.Extended.BattyRope = GetBool(b);
        scheme.Extended.RopeRollDrops = GetEnum<RopeRollDrops>(b);
        scheme.Extended.KeepControlXImpact = GetEnum<XImpactControlLoss>(b);
        scheme.Extended.KeepControlHeadBump = GetBool(b);
        scheme.Extended.KeepControlSkim = GetEnum<SkimControlLoss>(b);
        scheme.Extended.ExplosionFallDamage = GetBool(b);
        scheme.Extended.ObjectPushByExplosion = GetNullableBool(b);
        scheme.Extended.UndeterminedCrates = GetNullableBool(b);
        scheme.Extended.UndeterminedMineFuse = GetNullableBool(b);
        scheme.Extended.FiringPausesTimer = GetBool(b);
        scheme.Extended.LoseControlDoesntEndTurn = GetBool(b);
        scheme.Extended.ShotDoesntEndTurn = GetBool(b);
        scheme.Extended.ShotDoesntEndTurnAll = GetBool(b);
        scheme.Extended.DrillImpartsVelocity = GetNullableBool(b);
        scheme.Extended.GirderRadiusAssist = GetBool(b);
        scheme.Extended.FlameTurnDecay = GetFloat(b);
        scheme.Extended.FlameTouchDecay = GetByte(b);
        scheme.Extended.FlameLimit = GetByte(b);
        scheme.Extended.ProjectileMaxSpeed = GetByte(b);
        scheme.Extended.RopeMaxSpeed = GetByte(b);
        scheme.Extended.JetpackMaxSpeed = GetByte(b);
        scheme.Extended.GameSpeed = GetByte(b);
        scheme.Extended.IndianRopeGlitch = GetNullableBool(b);
        scheme.Extended.HerdDoublingGlitch = GetNullableBool(b);
        scheme.Extended.JetpackBungeeGlitch = GetBool(b);
        scheme.Extended.AngleCheatGlitch = GetBool(b);
        scheme.Extended.GlideGlitch = GetBool(b);
        scheme.Extended.SkipWalk = GetEnum<SkipWalk>(b);
        scheme.Extended.Roofing = GetEnum<Roofing>(b);
        scheme.Extended.FloatingWeaponGlitch = GetBool(b);
        scheme.Extended.WormBounce = GetFloat(b);
        scheme.Extended.Viscosity = GetFloat(b);
        scheme.Extended.ViscosityWorms = GetBool(b);
        scheme.Extended.RwWind = GetFloat(b);
        scheme.Extended.RwWindWorms = GetBool(b);
        scheme.Extended.RwGravityType = GetEnum<RwGravityType>(b);
        scheme.Extended.RwGravity = GetFloat(b);
        scheme.Extended.CrateRate = GetByte(b);
        scheme.Extended.CrateShower = GetBool(b);
        scheme.Extended.AntiSink = GetBool(b);
        scheme.Extended.WeaponsDontChange = GetBool(b);
        scheme.Extended.ExtendedFuse = GetBool(b);
        scheme.Extended.AutoReaim = GetBool(b);
        scheme.Extended.TerrainOverlapGlitch = GetNullableBool(b);
        scheme.Extended.RoundTimeFractional = GetBool(b);
        scheme.Extended.AutoRetreat = GetBool(b);
        scheme.Extended.HealthCure = GetEnum<HealthCure>(b);
        scheme.Extended.KaosMod = GetByte(b);
        scheme.Extended.SheepHeavenFlags = GetEnum<SheepHeavenFlags>(b);
        scheme.Extended.ConserveUtilities = GetBool(b);
        scheme.Extended.ExpediteUtilities = GetBool(b);
        scheme.Extended.DoubleTimeCount = GetByte(b);

        return scheme;
    }

    private static byte GetByte(TextReader b) => (byte) GetInt(b);

    private static byte? GetNullableByte(TextReader b)
    {
        var value = GetValue(ReadNextLine(b));

        return string.IsNullOrEmpty(value) ? null : (byte) GetInt(b);
    }

    private static sbyte GetSbyte(TextReader b) => (sbyte) GetInt(b);

    private static bool GetBool(TextReader b) => bool.Parse(GetValue(ReadNextLine(b)));

    private static bool? GetNullableBool(TextReader b)
    {
        var value = GetValue(ReadNextLine(b));

        return string.IsNullOrEmpty(value) ? null : bool.Parse(value);
    }

    private static int GetInt(TextReader b) => int.Parse(GetValue(ReadNextLine(b)), CultureInfo.CurrentCulture);

    private static float GetFloat(TextReader b) => float.Parse(GetValue(ReadNextLine(b)), CultureInfo.CurrentCulture);

    private static (sbyte, byte, sbyte, sbyte) GetWeaponDetails(TextReader b)
    {
        var line = ReadNextLine(b);
        var ammo = (sbyte) int.Parse(GetValue(line[..44]), CultureInfo.CurrentCulture);
        var power = (byte) int.Parse(GetValue(line.Substring(44, 10)), CultureInfo.CurrentCulture);
        var delay = (sbyte) int.Parse(GetValue(line.Substring(55, 20)), CultureInfo.CurrentCulture);
        var prob = (sbyte) int.Parse(GetValue(line[75..]), CultureInfo.CurrentCulture);

        return (ammo, power, delay, prob);
    }

    private static T GetEnum<T>(TextReader b)
        where T : struct =>
        Enum.Parse<T>(GetValue(ReadNextLine(b)));

    private static string ReadNextLine(TextReader b) =>
        b.ReadLine() ?? throw new ArgumentException("Unexpected end of definition");

    private static string GetValue(string text)
    {
        var startIndex = text.IndexOf('[', StringComparison.CurrentCulture) + 1;
        var endIndex = text.IndexOf(']', StringComparison.CurrentCulture);
        return text[startIndex..endIndex];
    }
}
