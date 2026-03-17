using System.Globalization;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed partial class DamageParser : IReplayLineParser
{
    private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
    private const string DamageDealtDetails = "(.+)";
    private const string Number = @"(\d+)";
    private const string TeamName = "(.+)";

    [GeneratedRegex($"{Timestamp} (•••|���) Damage dealt: {DamageDealtDetails}")]
    private static partial Regex DamageDealt();

    [GeneratedRegex($@"{Number} to {TeamName}")]
    private static partial Regex DamageWithNoKills();

    [GeneratedRegex($@"{Number} \({Number} kills?\) to {TeamName}")]
    private static partial Regex DamageWithKills();

    public bool CanParse(string line) => DamageDealt().IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var damageDealt = DamageDealt().Match(line);

        if (damageDealt.Success)
        {
            _ = builder.CurrentTurn.WithEndTime(
                TimeSpan.Parse(damageDealt.Groups[1].Value, CultureInfo.CurrentCulture));

            foreach (var damageDetail in damageDealt.Groups[3].Value.Split(','))
            {
                var damageWithNoKills = DamageWithNoKills().Match(damageDetail);
                var damageWithKills = DamageWithKills().Match(damageDetail);

                if (damageWithNoKills.Success)
                {
                    var teamName = GetTeamNameFromText(damageWithNoKills.Groups[2].Value);
                    var damageDone = uint.Parse(damageWithNoKills.Groups[1].Value, CultureInfo.CurrentCulture);
                    _ = builder.CurrentTurn.WithDamage(new Damage(builder.GetTeamByName(teamName), damageDone, 0));
                }
                else if (damageWithKills.Success)
                {
                    var teamName = GetTeamNameFromText(damageWithKills.Groups[3].Value);
                    var damageDone = uint.Parse(damageWithKills.Groups[1].Value, CultureInfo.CurrentCulture);
                    var kills = uint.Parse(damageWithKills.Groups[2].Value, CultureInfo.CurrentCulture);
                    _ = builder.CurrentTurn.WithDamage(new Damage(builder.GetTeamByName(teamName), damageDone, kills));
                }
            }
        }
    }

    private static string GetTeamNameFromText(string text) => text.Split('(')[0].Trim();
}
