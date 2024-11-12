using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Tests.Schemes.TestData;

internal static partial class TestSchemes
{
    public static Scheme Scheme19010101() =>
        new()
        {
            Weapons =
            {
                [Weapon.ClusterBomb] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 3,
                    Delay = 2,
                    Prob = 0
                },
                [Weapon.Uzi] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 2,
                    Delay = 3,
                    Prob = 0
                },
                [Weapon.Airstrike] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 11,
                    Delay = 0,
                    Prob = 0
                },
                [Weapon.Prod] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 2,
                    Delay = 3,
                    Prob = 0
                },
                [Weapon.Teleport] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 4,
                    Prob = 0
                },
                [Weapon.BaseballBat] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 3,
                    Delay = 4,
                    Prob = 0
                },
                [Weapon.LowGravity] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 4,
                    Prob = 0
                },
                [Weapon.FastWalk] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 3,
                    Prob = 0
                },
                [Weapon.SelectWorm] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 3,
                    Prob = 0
                },
                [Weapon.Invisibility] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 2,
                    Prob = 0
                },
                [Weapon.Freeze] = new SchemeWeapon
                {
                    Ammo = 1,
                    Power = 0,
                    Delay = 4,
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
