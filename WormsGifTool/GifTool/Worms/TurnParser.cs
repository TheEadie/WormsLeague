using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GifTool.Worms
{
    internal class TurnParser : ITurnParser
    {
        private static readonly string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
        private static readonly string Team = @"(.+\(.+\))";
        private static readonly string Weapon = @"(.+)";

        private static readonly Regex StartOfTurn = new Regex($"{Timestamp} ••• {Team} starts turn");
        private static readonly Regex WeaponUsage = new Regex($"{Timestamp} ••• {Team} fires {Weapon}");
        private static readonly Regex EndOfTurn = new Regex($"{Timestamp} ••• {Team} .*; time used:.*sec");
        private static readonly Regex DamageDealt = new Regex($"{Timestamp} ••• Damage dealt:");

        public Turn[] ParseTurns(string turnFileContents)
        {
            var turns = new List<Turn>();
            var weapons = new List<Turn.Action>();
            var startTime = TimeSpan.MinValue;
            var endTime = TimeSpan.MinValue;
            string team = null;

            foreach (var line in turnFileContents.Split('\n'))
            {
                var startMatch = StartOfTurn.Match(line);
                var weaponMatch = WeaponUsage.Match(line);
                var endMatch = EndOfTurn.Match(line);
                var damageDealt = DamageDealt.Match(line);

                if (startMatch.Success)
                {
                    if (team != null)
                    {
                        turns.Add(new Turn(team, weapons.ToArray(), startTime, endTime));
                        weapons.Clear();
                    }

                    startTime = TimeSpan.Parse(startMatch.Groups[1].Value);
                    team = startMatch.Groups[2].Value.Trim();
                }
                else if (weaponMatch.Success)
                {
                    var weaponTime = TimeSpan.Parse(weaponMatch.Groups[1].Value);
                    var weaponName = weaponMatch.Groups[3].Value.Trim();
                    weapons.Add(new Turn.Action(weaponTime, weaponName));
                }
                else if (endMatch.Success)
                {
                    endTime = TimeSpan.Parse(endMatch.Groups[1].Value);
                }
                else if(damageDealt.Success)
                {
                    endTime = TimeSpan.Parse(damageDealt.Groups[1].Value);
                }
            }

            if (team != null)
            {
                turns.Add(new Turn(team, weapons.ToArray(), startTime, endTime));
            }
            return turns.ToArray();
        }
    }
}
