using System.Globalization;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed partial class WeaponUsedParser : IReplayLineParser
{
    private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
    private const string TeamName = "(.+)";
    private const string WeaponName = "(.+)";
    private const string Number = @"(\d+)";
    private const string Modifiers = "(.+)";

    [GeneratedRegex($@"{Timestamp} (•••|���) {TeamName} fires {WeaponName} \({Number} sec, {Modifiers}\)")]
    private static partial Regex WeaponUsageWithFuseAndModifier();

    [GeneratedRegex($@"{Timestamp} (•••|���) {TeamName} fires {WeaponName} \({Number} sec\)")]
    private static partial Regex WeaponUsageWithFuse();

    [GeneratedRegex($@"{Timestamp} (•••|���) {TeamName} fires {WeaponName} \({Modifiers}\)")]
    private static partial Regex WeaponUsageWithModifier();

    [GeneratedRegex($"{Timestamp} (•••|���) {TeamName} fires {WeaponName}")]
    private static partial Regex WeaponUsage();

    public bool CanParse(string line) =>
        WeaponUsageWithFuseAndModifier().IsMatch(line) || WeaponUsageWithFuse().IsMatch(line) || WeaponUsage().IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var weaponUsedWithFuseAndModifier = WeaponUsageWithFuseAndModifier().Match(line);
        var weaponUsedWithFuse = WeaponUsageWithFuse().Match(line);
        var weaponUsedWithModifier = WeaponUsageWithModifier().Match(line);
        var weaponUsed = WeaponUsage().Match(line);

        if (weaponUsedWithFuseAndModifier.Success)
        {
            _ = builder.CurrentTurn.WithWeapon(
                new Weapon(
                    weaponUsedWithFuseAndModifier.Groups[4].Value.Trim(),
                    uint.Parse(weaponUsedWithFuseAndModifier.Groups[5].Value, CultureInfo.CurrentCulture),
                    weaponUsedWithFuseAndModifier.Groups[6].Value));
        }
        else if (weaponUsedWithFuse.Success)
        {
            _ = builder.CurrentTurn.WithWeapon(
                new Weapon(
                    weaponUsedWithFuse.Groups[4].Value.Trim(),
                    uint.Parse(weaponUsedWithFuse.Groups[5].Value, CultureInfo.CurrentCulture),
                    null));
        }
        else if (weaponUsedWithModifier.Success)
        {
            _ = builder.CurrentTurn.WithWeapon(
                new Weapon(
                    weaponUsedWithModifier.Groups[4].Value.Trim(),
                    null,
                    weaponUsedWithModifier.Groups[5].Value));
        }
        else if (weaponUsed.Success)
        {
            _ = builder.CurrentTurn.WithWeapon(new Weapon(weaponUsed.Groups[4].Value.Trim(), null, null));
        }
    }
}
