using NUnit.Framework;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Tests.Schemes;

public static class TestSchemes
{
    public static IEnumerable<TestCaseData> Schemes()
    {
        foreach (var property in typeof(Scheme).GetProperties().Where(x => x.CanWrite))
        {
            var scheme = new Scheme { ObjectCount = 10 };

            if (property.Name is "Weapons" or "RwVersion" or "Attachment" or "SchemeEditor")
            {
                // These are not supported by the text format
                continue;
            }

            if (property.PropertyType == typeof(bool))
            {
                property.SetValue(scheme, true);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(scheme, 10);
            }
            else if (property.PropertyType == typeof(byte))
            {
                property.SetValue(scheme, (byte) 10);
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
            else if (property.PropertyType == typeof(SchemeVersion))
            {
                property.SetValue(scheme, SchemeVersion.Version3);
            }
            else
            {
                throw new NotImplementedException($"Unknown property type {property.PropertyType} for {property.Name}");
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
            else if (property.PropertyType == typeof(bool?))
            {
                property.SetValue(scheme.Extended, true);
            }
            else if (property.PropertyType == typeof(byte))
            {
                property.SetValue(scheme.Extended, (byte) 5);
            }
            else if (property.PropertyType == typeof(byte?))
            {
                property.SetValue(scheme.Extended, (byte?) 5);
            }
            else if (property.PropertyType == typeof(uint))
            {
                property.SetValue(scheme.Extended, (uint) 10);
            }
            else if (property.PropertyType == typeof(short))
            {
                property.SetValue(scheme.Extended, (short) 10);
            }
            else if (property.PropertyType == typeof(ushort))
            {
                property.SetValue(scheme.Extended, (ushort) 10);
            }
            else if (property.PropertyType == typeof(float))
            {
                property.SetValue(scheme.Extended, 0.5f);
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
                throw new NotImplementedException($"Unknown property type {property.PropertyType} for {property.Name}");
            }

            yield return new TestCaseData(scheme).SetName(property.Name);
        }
    }
}
