using System.Collections.ObjectModel;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes;

public static class WeaponUtils
{
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

    private static readonly Weapon[] Movement =
    [
        Weapon.NinjaRope,
        Weapon.Parachute,
        Weapon.Bungee
    ];

    public static bool IsMovement(this Weapon weapon) => Movement.Contains(weapon);

    public static bool IsUtility(this Weapon weapon) => Utilities.Contains(weapon);

    public static bool IsRegularWeapon(this Weapon weapon) =>
        !IsUtility(weapon) && !IsMovement(weapon) && !weapon.IsSuperWeapon();

    public static readonly IDictionary<Weapon, byte[]> ValidPowerSettings = new ReadOnlyDictionary<Weapon, byte[]>(
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
            { Weapon.Armageddon, new byte[] { 0 } }
        });
}
