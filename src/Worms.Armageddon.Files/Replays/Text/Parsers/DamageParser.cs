using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal class DamageParser : IReplayLineParser
{
    private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
    private const string DamageDealtDetails = @"(.+)";
    private const string Number = @"(\d+)";
    private const string TeamName = @"(.+)";
    private static readonly Regex DamageDealt = new($"{Timestamp} (•••|���) Damage dealt: {DamageDealtDetails}");

    public bool CanParse(string line) => DamageDealt.IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var damageDealt = DamageDealt.Match(line);

        if (damageDealt.Success)
        {
            _ = builder.CurrentTurn.WithEndTime(TimeSpan.Parse(damageDealt.Groups[1].Value));

            var damageDetails = damageDealt.Groups[3].Value.Split(',');
            foreach (var damageDetail in damageDetails)
            {
                var damageWithNoKills = new Regex(@$"{Number} to {TeamName}").Match(damageDetail);
                var damageWithKills = new Regex(@$"{Number} \({Number} kill\) to {TeamName}").Match(damageDetail);

                if (damageWithNoKills.Success)
                {
                    var teamName = GetTeamNameFromText(damageWithNoKills.Groups[2].Value);
                    var damageDone = uint.Parse(damageWithNoKills.Groups[1].Value);
                    _ = builder.CurrentTurn.WithDamage(new Damage(builder.GetTeamByName(teamName), damageDone, 0));
                }
                else if (damageWithKills.Success)
                {
                    var teamName = GetTeamNameFromText(damageWithKills.Groups[3].Value);
                    var damageDone = uint.Parse(damageWithKills.Groups[1].Value);
                    var kills = uint.Parse(damageWithKills.Groups[2].Value);
                    _ = builder.CurrentTurn.WithDamage(new Damage(builder.GetTeamByName(teamName), damageDone, kills));
                }
            }
        }
    }

    private static string GetTeamNameFromText(string text) => text.Split('(')[0].Trim();
}
