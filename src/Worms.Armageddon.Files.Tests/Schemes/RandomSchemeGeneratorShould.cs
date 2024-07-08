using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes;
using Worms.Armageddon.Files.Schemes.Random;
using Worms.Armageddon.Files.Tests.Schemes.TestData;

namespace Worms.Armageddon.Files.Tests.Schemes;

public class RandomSchemeGeneratorShould
{
    private readonly IRandomSchemeGenerator _generator;

    public RandomSchemeGeneratorShould()
    {
        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonFilesServices();
        var serviceProvider = services.BuildServiceProvider();
        _generator = serviceProvider.GetRequiredService<IRandomSchemeGenerator>();
    }

    [Test]
    public void Select4StartingWeapons()
    {
        var scheme = _generator.Generate();
        var total = WeaponUtils.AllWeapons().Where(x => x.IsRegularWeapon()).Count(x => scheme.Weapons[x].Ammo == 1);
        total.ShouldBe(4);
    }

    [Test]
    public void Select5PowerfulWeapons()
    {
        var scheme = _generator.Generate();

        var found = WeaponUtils.AllWeapons()
            .Where(
                x => x.IsRegularWeapon()
                    && x.GetPowerSettings().Length > 1
                    && scheme.Weapons[x].Power == x.GetPowerSettings().Last());

        var total = found.Count();

        total.ShouldBeGreaterThanOrEqualTo(5);
    }

    [Test]
    public void AlwaysHaveUnlimitedMovementWeapons()
    {
        var scheme = _generator.Generate();
        scheme.Weapons[Weapon.NinjaRope].Ammo.ShouldBe((sbyte) 10);
        scheme.Weapons[Weapon.Bungee].Ammo.ShouldBe((sbyte) 10);
        scheme.Weapons[Weapon.Parachute].Ammo.ShouldBe((sbyte) 10);
    }

    private static IEnumerable<TestCaseData> SchemesWithSeeds =>
    [
        new TestCaseData(0, TestSchemes.SchemeSeed0()),
        new TestCaseData(19010101, TestSchemes.Scheme19010101())
    ];

    [Test]
    [TestCaseSource(nameof(SchemesWithSeeds))]
    public void RandomizeWeaponAmmoBasedOnSeed(int seed, Scheme expected)
    {
        var scheme = _generator.Generate(seed);

        _ = expected.ShouldNotBeNull();
        foreach (var weapon in WeaponUtils.AllWeapons())
        {
            scheme.Weapons[weapon].Ammo.ShouldBe(expected.Weapons[weapon].Ammo, $"Weapon name: {weapon}");
        }
    }

    [Test]
    [TestCaseSource(nameof(SchemesWithSeeds))]
    public void RandomizeWeaponPowerBasedOnSeed(int seed, Scheme expected)
    {
        var scheme = _generator.Generate(seed);

        _ = expected.ShouldNotBeNull();
        foreach (var weapon in WeaponUtils.AllWeapons())
        {
            if (scheme.Weapons[weapon].Ammo == 0)
            {
                continue;
            }

            scheme.Weapons[weapon].Power.ShouldBe(expected.Weapons[weapon].Power, $"Weapon name: {weapon}");
        }
    }

    [Test]
    [TestCaseSource(nameof(SchemesWithSeeds))]
    public void RandomizeWeaponDelayBasedOnSeed(int seed, Scheme expected)
    {
        var scheme = _generator.Generate(seed);

        _ = expected.ShouldNotBeNull();
        foreach (var weapon in WeaponUtils.AllWeapons())
        {
            if (scheme.Weapons[weapon].Ammo == 0)
            {
                continue;
            }

            scheme.Weapons[weapon].Delay.ShouldBe(expected.Weapons[weapon].Delay, $"Weapon name: {weapon}");
        }
    }
}
