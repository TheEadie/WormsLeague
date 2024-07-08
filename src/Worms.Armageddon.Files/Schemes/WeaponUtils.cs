using System.Collections.ObjectModel;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes;

public static class WeaponUtils
{
    public static IEnumerable<Weapon> AllWeapons() => Enum.GetValues<Weapon>();

    public static bool IsMovement(this Weapon weapon) => Movement.Contains(weapon);

    public static bool IsUtility(this Weapon weapon) => Utilities.Contains(weapon);

    public static bool IsRegularWeapon(this Weapon weapon) =>
        !IsUtility(weapon) && !IsMovement(weapon) && !weapon.IsSuperWeapon();

    public static byte[] GetPowerSettings(this Weapon weapon) => ValidPowerSettings[weapon];

    private static readonly Weapon[] Movement =
    [
        Weapon.NinjaRope,
        Weapon.Parachute,
        Weapon.Bungee
    ];

    private static readonly Weapon[] Utilities =
    [
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
    ];

    private static readonly IDictionary<Weapon, byte[]> ValidPowerSettings = new ReadOnlyDictionary<Weapon, byte[]>(
        new Dictionary<Weapon, byte[]>
        {
            {
                Weapon.Bazooka, [
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
                ]
            },
            {
                Weapon.HomingMissile, [
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
                ]
            },
            {
                Weapon.Mortar, [
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
                ]
            },
            {
                Weapon.Grenade, [
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
                ]
            },
            {
                Weapon.ClusterBomb, [
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
                ]
            },
            {
                Weapon.Skunk, [
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
                ]
            },
            {
                Weapon.PetrolBomb, [
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
                ]
            },
            {
                Weapon.BananaBomb, [
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
                ]
            },
            {
                Weapon.Handgun, [
                    10,
                    0,
                    2,
                    4,
                    13,
                    14
                ]
            },
            {
                Weapon.Shotgun, [
                    10,
                    0,
                    1,
                    2,
                    3,
                    4,
                    18,
                    13,
                    14
                ]
            },
            {
                Weapon.Uzi, [
                    10,
                    0,
                    2,
                    4,
                    13,
                    14
                ]
            },
            {
                Weapon.Minigun, [
                    10,
                    0,
                    2,
                    4,
                    13,
                    14
                ]
            },
            {
                Weapon.Longbow, [
                    0,
                    1,
                    2,
                    3,
                    4
                ]
            },
            {
                Weapon.Airstrike, [
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
                ]
            },
            {
                Weapon.NapalmStrike, [
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
                ]
            },
            {
                Weapon.Mine, [
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
                ]
            },
            {
                Weapon.Firepunch, [
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
                ]
            },
            {
                Weapon.Dragonball, [
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
                ]
            },
            {
                Weapon.Kamikaze, [
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
                ]
            },
            { Weapon.Prod, [2] },
            {
                Weapon.BattleAxe, [
                    0,
                    1,
                    2,
                    3,
                    4
                ]
            },
            {
                Weapon.Blowtorch, [
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
                ]
            },
            {
                Weapon.PneumaticDrill, [
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
                ]
            },
            { Weapon.Girder, [3] },
            { Weapon.NinjaRope, [4] },
            { Weapon.Parachute, [0] },
            { Weapon.Bungee, [0] },
            { Weapon.Teleport, [0] },
            {
                Weapon.Dynamite, [
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
                ]
            },
            {
                Weapon.Sheep, [
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
                ]
            },
            {
                Weapon.BaseballBat, [
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
                ]
            },
            {
                Weapon.Flamethrower, [
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
                ]
            },
            {
                Weapon.HomingPigeon, [
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
                ]
            },
            {
                Weapon.MadCow, [
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
                ]
            },
            {
                Weapon.HolyHandGrenade, [
                    255,
                    0,
                    1,
                    2,
                    3
                ]
            },
            {
                Weapon.OldWoman, [
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
                ]
            },
            {
                Weapon.SheepLauncher, [
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
                ]
            },
            {
                Weapon.SuperSheep, [
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
                ]
            },
            {
                Weapon.MoleBomb, [
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
                ]
            },
            { Weapon.Jetpack, [0] },
            { Weapon.LowGravity, [0] },
            { Weapon.LaserSight, [0] },
            { Weapon.FastWalk, [0] },
            { Weapon.Invisibility, [0] },
            { Weapon.DamageX2, [0] },
            { Weapon.Freeze, [0] },
            { Weapon.SuperBananaBomb, [0] },
            { Weapon.MineStrike, [0] },
            { Weapon.GirderStarterPack, [0] },
            { Weapon.Earthquake, [0] },
            { Weapon.ScalesOfJustice, [0] },
            { Weapon.MingVase, [0] },
            { Weapon.MikesCarpetBomb, [0] },
            { Weapon.MagicBullet, [0] },
            { Weapon.NuclearTest, [0] },
            { Weapon.SelectWorm, [0] },
            { Weapon.SalvationArmy, [0] },
            { Weapon.MoleSquadron, [0] },
            { Weapon.MBBomb, [0] },
            { Weapon.ConcreteDonkey, [0] },
            { Weapon.SuicideBomber, [0] },
            { Weapon.SheepStrike, [0] },
            { Weapon.MailStrike, [0] },
            { Weapon.Armageddon, [0] }
        });
}
