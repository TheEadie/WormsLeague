﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Resources.Replays.Text
{
    internal class ReplayTextReader : IReplayTextReader
    {
        private const string DateAndTime = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})";
        private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";

        private const string TeamColour = @"(Red|Blue|Green|Yellow|Magenta|Cyan)";
        private const string TeamName = @"(.+)";
        private const string PlayerName = @"(.+)";
        private const string WeaponName = @"(.+)";
        private const string Number = @"(\d+)";
        private const string Modifiers = @"(.+)";
        private const string DamageDealtDetails = @"(.+)";

        private static readonly Regex StartTime = new Regex($"Game Started at {DateAndTime} GMT");
        private static readonly Regex TeamSummaryOnline = new Regex($"{TeamColour}:.*\"{PlayerName}\" as .*\"{TeamName}\"");
        private static readonly Regex TeamSummaryOffline = new Regex($"{TeamColour}:.*\"{TeamName}\"");

        private static readonly Regex WinnerDraw = new Regex($"The round was drawn.");
        private static readonly Regex Winner = new Regex($"{TeamName} wins the (match!|round.)");

        private static readonly Regex StartOfTurn = new($"{Timestamp} ••• {TeamName} starts turn");
        private static readonly Regex EndOfTurn = new($"{Timestamp} ••• {TeamName} .*; time used:.*sec");
        private static readonly Regex DamageDealt = new Regex($"{Timestamp} ••• Damage dealt: {DamageDealtDetails}");

        private static readonly Regex WeaponUsageWithFuseAndModifier = new Regex($@"{Timestamp} ••• {TeamName} fires {WeaponName} \({Number} sec, {Modifiers}\)");
        private static readonly Regex WeaponUsageWithFuse = new Regex($@"{Timestamp} ••• {TeamName} fires {WeaponName} \({Number} sec\)");
        private static readonly Regex WeaponUsage = new Regex($@"{Timestamp} ••• {TeamName} fires {WeaponName}");

        public ReplayResource GetModel(string definition)
        {
            var startTime = DateTime.MinValue;
            var teams = new List<Team>();
            var winner = string.Empty;
            var turns = new List<Turn>();
            var currentTurn = new TurnBuilder();

            foreach (var line in definition.Split('\n'))
            {
                var startTimeMatch = StartTime.Match(line);
                var teamSummaryOnlineMatch = TeamSummaryOnline.Match(line);
                var teamSummaryOfflineMatch = TeamSummaryOffline.Match(line);
                var winnerDrawMatch = WinnerDraw.Match(line);
                var winnerMatch = Winner.Match(line);
                var startOfTurn = StartOfTurn.Match(line);
                var endOfTurn = EndOfTurn.Match(line);
                var damageDealt = DamageDealt.Match(line);
                var weaponUsedWithFuseAndModifier = WeaponUsageWithFuseAndModifier.Match(line);
                var weaponUsedWithFuse = WeaponUsageWithFuse.Match(line);
                var weaponUsed = WeaponUsage.Match(line);

                if (startTimeMatch.Success)
                {
                    startTime = DateTime.Parse(startTimeMatch.Groups[1].Value);
                }

                if (teamSummaryOnlineMatch.Success)
                {
                    teams.Add(Team.Remote(
                        teamSummaryOnlineMatch.Groups[3].Value,
                        teamSummaryOnlineMatch.Groups[2].Value,
                        Enum.Parse<TeamColour>(teamSummaryOnlineMatch.Groups[1].Value)));
                }
                else if (teamSummaryOfflineMatch.Success)
                {
                    teams.Add(Team.Local(
                        teamSummaryOfflineMatch.Groups[2].Value,
                        Enum.Parse<TeamColour>(teamSummaryOfflineMatch.Groups[1].Value)));
                }

                if (winnerDrawMatch.Success)
                {
                    winner = "Draw";
                }
                else if (winnerMatch.Success)
                {
                    winner = winnerMatch.Groups[1].Value;
                }

                if (startOfTurn.Success)
                {
                    var startTimeTurn = TimeSpan.Parse(startOfTurn.Groups[1].Value);
                    var teamName = GetTeamNameFromText(startOfTurn.Groups[2].Value);

                    AddTurnIfAnyDetailsSet(currentTurn, turns);
                    currentTurn = new TurnBuilder()
                        .WithStartTime(startTimeTurn)
                        .WithTeam(teams.Single(x => x.Name == teamName));
                }
                else if (weaponUsedWithFuseAndModifier.Success)
                {
                    currentTurn.WithWeapon(
                        new Weapon(
                            weaponUsedWithFuseAndModifier.Groups[3].Value.Trim(),
                            uint.Parse(weaponUsedWithFuseAndModifier.Groups[4].Value),
                            weaponUsedWithFuseAndModifier.Groups[5].Value));
                }
                else if (weaponUsedWithFuse.Success)
                {
                    currentTurn.WithWeapon(
                        new Weapon(
                            weaponUsedWithFuse.Groups[3].Value.Trim(),
                            uint.Parse(weaponUsedWithFuse.Groups[4].Value),
                            null));
                }
                else if (weaponUsed.Success)
                {
                    currentTurn.WithWeapon(
                        new Weapon(
                            weaponUsed.Groups[3].Value.Trim(),
                            null,
                            null));
                }
                else if (endOfTurn.Success)
                {
                    currentTurn.WithEndTime(TimeSpan.Parse(endOfTurn.Groups[1].Value));
                }
                else if (damageDealt.Success)
                {
                    currentTurn.WithEndTime(TimeSpan.Parse(damageDealt.Groups[1].Value));

                    var damageDetails = damageDealt.Groups[2].Value.Split(',');
                    foreach (var damageDetail in damageDetails)
                    {
                        var damageWithNoKills = new Regex(@$"{Number} to {TeamName}").Match(damageDetail);
                        var damageWithKills = new Regex(@$"{Number} \({Number} kill\) to {TeamName}").Match(damageDetail);

                        if (damageWithNoKills.Success)
                        {
                            var teamName = GetTeamNameFromText(damageWithNoKills.Groups[2].Value);
                            var damageDone = uint.Parse(damageWithNoKills.Groups[1].Value);
                            currentTurn.WithDamage(new Damage(teams.Single(x => x.Name == teamName), damageDone, 0));
                        }
                        else if (damageWithKills.Success)
                        {
                            var teamName = GetTeamNameFromText(damageWithKills.Groups[3].Value);
                            var damageDone = uint.Parse(damageWithKills.Groups[1].Value);
                            var kills = uint.Parse(damageWithKills.Groups[2].Value);
                            currentTurn.WithDamage(new Damage(teams.Single(x => x.Name == teamName), damageDone, kills));
                        }
                    }
                }
            }

            AddTurnIfAnyDetailsSet(currentTurn, turns);

            return new ReplayResource(
                startTime,
                "local",
                true,
                teams,
                winner,
                turns,
                definition);
        }

        private static void AddTurnIfAnyDetailsSet(TurnBuilder currentTurn, ICollection<Turn> turns)
        {
            if (!currentTurn.HasRequiredDetails())
            {
                return;
            }

            turns.Add(currentTurn.Build());
        }

        private static string GetTeamNameFromText(string text)
        {
            return text.Split('(')[0].Trim();
        }
    }
}
