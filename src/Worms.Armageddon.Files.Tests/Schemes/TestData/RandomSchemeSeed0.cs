using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Tests.Schemes.TestData;

internal static partial class TestSchemes
{
    public static Scheme SchemeSeed0() =>
        new()
        {
            Weapons =
            {
                [Weapon.Girder] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 3,
                    Delay = 3,
                    Prob = 0
                },
                [Weapon.BaseballBat] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 10,
                    Delay = 0,
                    Prob = 0
                },
                [Weapon.HomingPigeon] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 4,
                    Delay = 5,
                    Prob = 0
                },
                [Weapon.Blowtorch] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 4,
                    Delay = 4,
                    Prob = 0
                },
                [Weapon.MadCow] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 0,
                    Prob = 0
                },
                [Weapon.LowGravity] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 1,
                    Prob = 0
                },
                [Weapon.FastWalk] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 5,
                    Prob = 0
                },
                [Weapon.SelectWorm] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 1,
                    Prob = 0
                },
                [Weapon.NinjaRope] = new SchemeWeapon
                {
                    Ammo = 10,
                    Power = 4,
                    Delay = 0,
                    Prob = 0
                },
                [Weapon.Parachute] = new SchemeWeapon
                {
                    Ammo = 10,
                    Power = 0,
                    Delay = 0,
                    Prob = 0
                },
                [Weapon.Bungee] = new SchemeWeapon
                {
                    Ammo = 10,
                    Power = 0,
                    Delay = 0,
                    Prob = 0
                }
            }
        };
}
