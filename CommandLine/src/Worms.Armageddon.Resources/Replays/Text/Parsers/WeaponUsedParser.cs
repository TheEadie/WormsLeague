using System.Text.RegularExpressions;

namespace Worms.Armageddon.Resources.Replays.Text.Parsers
{
    internal class WeaponUsedParser : IReplayLineParser
    {
        private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
        private const string TeamName = @"(.+)";
        private const string WeaponName = @"(.+)";
        private const string Number = @"(\d+)";
        private const string Modifiers = @"(.+)";
        private static readonly Regex WeaponUsageWithFuseAndModifier = new($@"{Timestamp} (•••|���) {TeamName} fires {WeaponName} \({Number} sec, {Modifiers}\)");
        private static readonly Regex WeaponUsageWithFuse = new($@"{Timestamp} (•••|���) {TeamName} fires {WeaponName} \({Number} sec\)");
        private static readonly Regex WeaponUsage = new($@"{Timestamp} (•••|���) {TeamName} fires {WeaponName}");

        public bool CanParse(string line) =>
            WeaponUsageWithFuseAndModifier.IsMatch(line) ||
            WeaponUsageWithFuse.IsMatch(line) ||
            WeaponUsage.IsMatch(line);

        public void Parse(string line, ReplayResourceBuilder builder)
        {
            var weaponUsedWithFuseAndModifier = WeaponUsageWithFuseAndModifier.Match(line);
            var weaponUsedWithFuse = WeaponUsageWithFuse.Match(line);
            var weaponUsed = WeaponUsage.Match(line);

            if (weaponUsedWithFuseAndModifier.Success)
            {
                builder.CurrentTurn.WithWeapon(
                    new Weapon(
                        weaponUsedWithFuseAndModifier.Groups[4].Value.Trim(),
                        uint.Parse(weaponUsedWithFuseAndModifier.Groups[5].Value),
                        weaponUsedWithFuseAndModifier.Groups[6].Value));
            }
            else if (weaponUsedWithFuse.Success)
            {
                builder.CurrentTurn.WithWeapon(
                    new Weapon(
                        weaponUsedWithFuse.Groups[4].Value.Trim(),
                        uint.Parse(weaponUsedWithFuse.Groups[5].Value),
                        null));
            }
            else if (weaponUsed.Success)
            {
                builder.CurrentTurn.WithWeapon(
                    new Weapon(
                        weaponUsed.Groups[4].Value.Trim(),
                        null,
                        null));
            }
        }
    }
}
