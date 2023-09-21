using NUnit.Framework;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Tests.Schemes;

public static class TestSchemes
{
    public static IEnumerable<TestCaseData> Schemes()
    {
        foreach (var property in typeof(Scheme).GetProperties())
        {
            var scheme = new Scheme { ObjectCount = 10 };

            if (property.PropertyType == typeof(bool))
            {
                property.SetValue(scheme, true);
            }
            else if (property.PropertyType == typeof(byte))
            {
                property.SetValue(scheme, (byte) 10);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(scheme, 10);
            }
            else if (property.PropertyType == typeof(sbyte))
            {
                property.SetValue(scheme, (sbyte) 10);
            }
            else if (property.PropertyType == typeof(Stockpiling))
            {
                property.SetValue(scheme, Stockpiling.On);
            }
            else if (property.PropertyType == typeof(WormSelect))
            {
                property.SetValue(scheme, WormSelect.Random);
            }
            else if (property.PropertyType == typeof(SuddenDeathEvent))
            {
                property.SetValue(scheme, SuddenDeathEvent.NuclearStrike);
            }
            else if (property.PropertyType == typeof(MapObjectType))
            {
                property.SetValue(scheme, MapObjectType.OilDrums);
            }
            else
            {
                continue;
            }

            yield return new TestCaseData(scheme).SetName(property.Name);
        }

        foreach (var property in typeof(Scheme.ExtendedOptions).GetProperties())
        {
            var scheme = new Scheme { ObjectCount = 10 };

            if (property.PropertyType == typeof(bool))
            {
                property.SetValue(scheme.Extended, true);
            }
            else if (property.PropertyType == typeof(byte))
            {
                property.SetValue(scheme.Extended, (byte) 5);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(scheme.Extended, 10);
            }
            else if (property.PropertyType == typeof(sbyte))
            {
                property.SetValue(scheme.Extended, (sbyte) 10);
            }
            else if (property.PropertyType == typeof(WormPhasing))
            {
                property.SetValue(scheme.Extended, WormPhasing.WormsWeapons);
            }
            else if (property.PropertyType == typeof(RopeRollDrops))
            {
                property.SetValue(scheme.Extended, RopeRollDrops.RopeJump);
            }
            else if (property.PropertyType == typeof(XImpactControlLoss))
            {
                property.SetValue(scheme.Extended, XImpactControlLoss.Loss);
            }
            else if (property.PropertyType == typeof(SkimControlLoss))
            {
                property.SetValue(scheme.Extended, SkimControlLoss.KeptWithRope);
            }
            else if (property.PropertyType == typeof(SkipWalk))
            {
                property.SetValue(scheme.Extended, SkipWalk.Facilitated);
            }
            else if (property.PropertyType == typeof(Roofing))
            {
                property.SetValue(scheme.Extended, Roofing.Up);
            }
            else if (property.PropertyType == typeof(RwGravityType))
            {
                property.SetValue(scheme.Extended, RwGravityType.BlackHoleConstant);
            }
            else if (property.PropertyType == typeof(HealthCure))
            {
                property.SetValue(scheme.Extended, HealthCure.Nobody);
            }
            else if (property.PropertyType == typeof(SheepHeavenFlags))
            {
                property.SetValue(scheme.Extended, SheepHeavenFlags.Odds);
            }
            else
            {
                continue;
            }

            yield return new TestCaseData(scheme).SetName(property.Name);
        }
    }
}
