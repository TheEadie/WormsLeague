using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Resources.Replays;
using Worms.Armageddon.Resources.Replays.Text;
using Worms.Armageddon.Resources.Replays.Text.Parsers;

namespace Worms.Armageddon.Resources.Tests.Replays
{
    public class ReplayTextReaderShould
    {
        private IReplayTextReader _replayTextReader;

        private readonly Team _redTeam = new("Red Team", "local", TeamColour.Red);
        private readonly Team _blueTeam = new("Blue Team", "local", TeamColour.Blue);
        private readonly Team _greenTeam = new("Green Team", "local", TeamColour.Green);
        private readonly Team _yellowTeam = new("1UP", "local", TeamColour.Yellow);
        private readonly Team _magentaTeam = new("Test Team", "local", TeamColour.Magenta);
        private readonly Team _cyanTeam = new("Last Team Name", "local", TeamColour.Cyan);

        [SetUp]
        public void Setup()
        {
            _replayTextReader = new ReplayTextReader(
                new List<IReplayLineParser>()
                {
                    new StartTimeParser(),
                    new TeamParser(),
                    new WinnerParser(),
                    new StartOfTurnParser(),
                    new WeaponUsedParser(),
                    new DamageParser(),
                    new EndOfTurnParser()
                });
        }

        [Test]
        public void ReadStartTimeFromReplay()
        {
            const string log =
                "Game Started at 2019-01-11 12:58:40 GMT";

            var replay = _replayTextReader.GetModel(log);

            replay.Date.ShouldBe(new DateTime(2019, 1, 11, 12, 58, 40));
        }

        [Test]
        public void ReadTeamsForOfflineMatch()
        {
            var log =
                $"Red: \"{_redTeam.Name}\"" + Environment.NewLine +
                $"Blue: \"{_blueTeam.Name}\"" + Environment.NewLine +
                $"Green: \"{_greenTeam.Name}\"" + Environment.NewLine +
                $"Yellow: \"{_yellowTeam.Name}\"" + Environment.NewLine +
                $"Magenta: \"{_magentaTeam.Name}\"" + Environment.NewLine +
                $"Cyan: \"{_cyanTeam.Name}\"" + Environment.NewLine;

            var replay = _replayTextReader.GetModel(log);

            replay.Teams.Count.ShouldBe(6);
            replay.Teams.ShouldContain(_redTeam);
            replay.Teams.ShouldContain(_blueTeam);
            replay.Teams.ShouldContain(_greenTeam);
            replay.Teams.ShouldContain(_yellowTeam);
            replay.Teams.ShouldContain(_magentaTeam);
            replay.Teams.ShouldContain(_cyanTeam);
        }

        [Test]
        public void ReadTeamsForOnlineMatch()
        {
            var log =
                $"Red: \"{_redTeam.Machine}\"     as \"{_redTeam.Name}\"" + Environment.NewLine +
                $"Blue: \"{_blueTeam.Machine}\"    as \"{_blueTeam.Name}\"" + Environment.NewLine +
                $"Green: \"{_greenTeam.Machine}\"   as \"{_greenTeam.Name}\"" + Environment.NewLine +
                $"Yellow: \"{_yellowTeam.Machine}\"  as \"{_yellowTeam.Name}\"" + Environment.NewLine +
                $"Magenta: \"{_magentaTeam.Machine}\" as \"{_magentaTeam.Name}\"" + Environment.NewLine +
                $"Cyan: \"{_cyanTeam.Machine}\"    as \"{_cyanTeam.Name}\"" + Environment.NewLine;

            var replay = _replayTextReader.GetModel(log);

            replay.Teams.Count.ShouldBe(6);
            replay.Teams.ShouldContain(_redTeam);
            replay.Teams.ShouldContain(_blueTeam);
            replay.Teams.ShouldContain(_greenTeam);
            replay.Teams.ShouldContain(_yellowTeam);
            replay.Teams.ShouldContain(_magentaTeam);
            replay.Teams.ShouldContain(_cyanTeam);
        }

        [Test]
        public void ReadWinnerOfMatch()
        {
            var log = $"{_redTeam.Name} wins the match!";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe(_redTeam.Name);
        }

        [Test]
        public void ReadWinnerOfRound()
        {
            var log = $"{_redTeam.Name} wins the round.";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe(_redTeam.Name);
        }

        [Test]
        public void ReadWinnerAsADraw()
        {
            const string log = "The round was drawn.";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe("Draw");
        }

        [Test]
        public void ReadTurnTeamFromOnlineGame()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Start.ShouldBe(new TimeSpan(0, 0, 6, 59, 80));
            replay.Turns.ElementAt(0).End.ShouldBe(new TimeSpan(0, 0, 7, 26, 600));
            replay.Turns.ElementAt(0).Team.Name.ShouldBe("Some Team");
            replay.Turns.ElementAt(0).Team.Machine.ShouldBe("a person");
        }

        [Test]
        public void ReadTurnTeamFromOfflineGame()
        {
            var log =
                "Red: \"Some Team\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Start.ShouldBe(new TimeSpan(0, 0, 6, 59, 80));
            replay.Turns.ElementAt(0).End.ShouldBe(new TimeSpan(0, 0, 7, 26, 600));
            replay.Turns.ElementAt(0).Team.Name.ShouldBe("Some Team");
        }

        [Test]
        public void ReadMultipleTurns()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "Blue: \"another person\" as \"Team 2\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat" + Environment.NewLine +
                "[00:09:59.08] ••• Team 2 (another person) starts turn" + Environment.NewLine +
                "[00:10:08.26] ••• Team 2 (another person) fires Shotgun" + Environment.NewLine +
                "[00:11:26.60] ••• Team 3 (another person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(2);
            replay.Turns.ElementAt(0).Team.Name.ShouldBe("Some Team");
            replay.Turns.ElementAt(1).Team.Name.ShouldBe("Team 2");
        }

        [Test]
        public void ReadEndOfTurnWithLossOfControl()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "Blue: \"another person\" as \"Team 2\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) loses turn due to loss of control; time used: 40.94 sec turn, 0.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Start.ShouldBe(new TimeSpan(0, 0, 6, 59, 80));
            replay.Turns.ElementAt(0).End.ShouldBe(new TimeSpan(0, 0, 7, 26, 600));
            replay.Turns.ElementAt(0).Team.Name.ShouldBe("Some Team");
        }

        [Test]
        public void ReadWeaponDetails()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Name.ShouldBe("Shotgun");
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Fuse.ShouldBe(null);
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Modifier.ShouldBe(null);
        }

        [Test]
        public void ReadWeaponDetailsForGrenades()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Grenade (3 sec, min bounce)" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Name.ShouldBe("Grenade");
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Fuse.ShouldBe(new uint?(3));
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Modifier.ShouldBe("min bounce");
        }

        [Test]
        public void ReadWeaponDetailsForBananaBomb()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Banana Bomb (3 sec)" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Name.ShouldBe("Banana Bomb");
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Fuse.ShouldBe(new uint?(3));
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Modifier.ShouldBe(null);
        }

        [Test]
        public void ReadWeaponDetailsWhenMultipleWeaponsUsed()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Ninja Rope" + Environment.NewLine +
                "[00:07:45.34] ••• Some Team (a person) fires Banana Bomb (3 sec)" + Environment.NewLine +
                "[00:08:35.87] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Weapons.Count.ShouldBe(2);
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Name.ShouldBe("Ninja Rope");
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Fuse.ShouldBe(null);
            replay.Turns.ElementAt(0).Weapons.ElementAt(0).Modifier.ShouldBe(null);
            replay.Turns.ElementAt(0).Weapons.ElementAt(1).Name.ShouldBe("Banana Bomb");
            replay.Turns.ElementAt(0).Weapons.ElementAt(1).Fuse.ShouldBe(new uint?(3));
            replay.Turns.ElementAt(0).Weapons.ElementAt(1).Modifier.ShouldBe(null);
        }

        [Test]
        public void ReadDamageToSingleTeam()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "Blue: \"another person\" as \"Team 2\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Damage dealt: 45 to Team 2 (another person)";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Damage.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Damage.ElementAt(0).Team.Name.ShouldBe("Team 2");
            replay.Turns.ElementAt(0).Damage.ElementAt(0).HealthLost.ShouldBe((uint) 45);
            replay.Turns.ElementAt(0).Damage.ElementAt(0).WormsKilled.ShouldBe((uint) 0);
        }

        [Test]
        public void ReadDamageToSingleTeamWithDeaths()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "Blue: \"another person\" as \"Team 2\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Damage dealt: 100 (1 kill) to Team 2 (another person)";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Damage.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Damage.ElementAt(0).Team.Name.ShouldBe("Team 2");
            replay.Turns.ElementAt(0).Damage.ElementAt(0).HealthLost.ShouldBe((uint) 100);
            replay.Turns.ElementAt(0).Damage.ElementAt(0).WormsKilled.ShouldBe((uint) 1);
        }

        [Test]
        public void ReadDamageToMultipleTeams()
        {
            var log =
                "Red: \"a person\" as \"Some Team\"" + Environment.NewLine +
                "Blue: \"another person\" as \"Team 2\"" + Environment.NewLine +
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Damage dealt: 42 to Some Team (a person), 100 (1 kill) to Team 2 (another person)";

            var replay = _replayTextReader.GetModel(log);

            replay.Turns.Count.ShouldBe(1);
            replay.Turns.ElementAt(0).Damage.Count.ShouldBe(2);
            replay.Turns.ElementAt(0).Damage.ElementAt(0).Team.Name.ShouldBe("Some Team");
            replay.Turns.ElementAt(0).Damage.ElementAt(0).HealthLost.ShouldBe((uint) 42);
            replay.Turns.ElementAt(0).Damage.ElementAt(0).WormsKilled.ShouldBe((uint) 0);
            replay.Turns.ElementAt(0).Damage.ElementAt(1).Team.Name.ShouldBe("Team 2");
            replay.Turns.ElementAt(0).Damage.ElementAt(1).HealthLost.ShouldBe((uint) 100);
            replay.Turns.ElementAt(0).Damage.ElementAt(1).WormsKilled.ShouldBe((uint) 1);
        }
    }
}
