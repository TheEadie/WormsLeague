using System;
using GifTool.Worms;
using NUnit.Framework;

namespace GifTool.Tests
{
    [TestFixture]
    public class TurnParserShould
    {
        private TurnParser _turnParser;

        [SetUp]
        public void SetUp()
        {
            _turnParser = new TurnParser();
        }

        [Test]
        public void ParseATurnWithSpacesInTeam()
        {
            var log =
                "[00:06:59.08] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:07:08.26] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:22.44] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var turns = _turnParser.ParseTurns(log);

            Assert.That(turns, Has.Length.EqualTo(1));
            Assert.That(turns[0].Team, Is.EqualTo("Some Team (a person)"));
            Assert.That(turns[0].WeaponActions, Has.Length.EqualTo(2));
            Assert.That(turns[0].StartTime, Is.EqualTo(new TimeSpan(0, 0, 6, 59, 80)));
            Assert.That(turns[0].EndTime, Is.EqualTo(new TimeSpan(0, 0, 7, 26, 600)));
        }

        [Test]
        public void ParseWeaponsFromTurn()
        {
            var log =
                "[00:01:01.11] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:01:02.22] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:01:03.33] ••• Some Team (a person) fires Homing Missile" + Environment.NewLine +
                "[00:01:04.44] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var turns = _turnParser.ParseTurns(log);

            Assert.That(turns, Has.Length.EqualTo(1));
            Assert.That(turns[0].Team, Is.EqualTo("Some Team (a person)"));
            Assert.That(turns[0].WeaponActions, Has.Length.EqualTo(2));
            Assert.That(turns[0].WeaponActions[0].Description, Is.EqualTo("Shotgun"));
            Assert.That(turns[0].WeaponActions[1].Description, Is.EqualTo("Homing Missile"));
            Assert.That(turns[0].WeaponActions[0].TimeStamp, Is.EqualTo(new TimeSpan(0, 0, 1, 2, 220)));
            Assert.That(turns[0].WeaponActions[1].TimeStamp, Is.EqualTo(new TimeSpan(0, 0, 1, 3, 330)));
        }

        [Test]
        public void IgnoreLinesFromChat()
        {
            var log =
                "[00:01:01.00] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:02:02.00] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:02:22.00] [a person] Chat message" + Environment.NewLine +
                "[00:03:03.00] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:03:33.00] [a person] Another (chat) message!" + Environment.NewLine +
                "[00:03:33.00] [a person] sneaky chat (message) ends turn;" + Environment.NewLine +
                "[00:03:33.00] [a person] sneaky chat (message) fires weapon" + Environment.NewLine +
                "[00:04:00.00] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:04:04.00] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat" + Environment.NewLine +
                "[00:04:04.22] ••• Damage dealt: 32 (1 kill) to Enemy Team (person 2)";

            var turns = _turnParser.ParseTurns(log);

            Assert.That(turns, Has.Length.EqualTo(1));
            Assert.That(turns[0].Team, Is.EqualTo("Some Team (a person)"));
            Assert.That(turns[0].WeaponActions, Has.Length.EqualTo(3));
        }

        [Test]
        public void ParseTurnWithLossOfControl()
        {
            var log =
                "[00:01:01.00] ••• Some Team (a person) starts turn" + Environment.NewLine +
                "[00:02:02.00] ••• Some Team (a person) fires Shotgun" + Environment.NewLine +
                "[00:04:04.00] ••• Some Team (a person) loses turn due to loss of control; time used: 40.94 sec turn, 0.00 sec retreat" + Environment.NewLine +
                "[00:04:04.22] ••• Damage dealt: 32 (1 kill) to Enemy Team (person 2)";

            var turns = _turnParser.ParseTurns(log);

            Assert.That(turns, Has.Length.EqualTo(1));
            Assert.That(turns[0].Team, Is.EqualTo("Some Team (a person)"));
            Assert.That(turns[0].WeaponActions, Has.Length.EqualTo(1));
        }

        [Test]
        public void IgnorePreamble()
        {
            var log = 
                "Game ID: \"extern\"" + Environment.NewLine +
                "nGame Started at 2019-01-11 12:58:40 GMT" + Environment.NewLine +
                "Game Engine Version: 3.7.2.1" + Environment.NewLine +
                "File Format Version: 3.6.29.48 - 3.7.2.2" + Environment.NewLine +
                "Exported with Version: 3.7.2.2" + Environment.NewLine +
                "" + Environment.NewLine +
                "Green:   \"player1\" as \"team1\"" + Environment.NewLine +
                "Blue:    \"player2\" as \"team2\"" + Environment.NewLine +
                "Cyan:    \"player3\" as \"team3\" [Host]" + Environment.NewLine +
                "Magenta: \"player4\" as \"team4\" [Host]" + Environment.NewLine +
                "Red:     \"player5\" as \"team5\"" + Environment.NewLine +
                "Yellow:  \"player6\" as \"team6\" [Local Player]" + Environment.NewLine +
                "" + Environment.NewLine +
                "[00:01:01.00] ••• team6 (player6) starts turn" + Environment.NewLine +
                "[00:02:02.00] ••• team6 (player6) fires Shotgun" + Environment.NewLine +
                "[00:03:03.00] ••• team6 (player6) fires Shotgun" + Environment.NewLine +
                "[00:04:04.00] ••• team6 (player6) ends turn; time used: 11.58 sec turn, 3.00 sec retreat" + Environment.NewLine +
                "[00:04:04.22] ••• Damage dealt: 32 (1 kill) to Enemy Team (person 2)";

            var turns = _turnParser.ParseTurns(log);

            Assert.That(turns, Has.Length.EqualTo(1));
            Assert.That(turns[0].Team, Is.EqualTo("team6 (player6)"));
            Assert.That(turns[0].WeaponActions, Has.Length.EqualTo(2));
        }

        [Test]
        public void ParseMultipleTurns()
        {
            var log =
                "[00:01:01.00] ••• team6 (player6) starts turn" + Environment.NewLine +
                "[00:02:02.00] ••• team6 (player6) fires Shotgun" + Environment.NewLine +
                "[00:03:03.00] ••• team6 (player6) fires Shotgun" + Environment.NewLine +
                "[00:04:04.00] ••• team6 (player6) ends turn; time used: 11.58 sec turn, 3.00 sec retreat" + Environment.NewLine +
                "[00:04:04.22] ••• Damage dealt: 32 (1 kill) to team2 (player2)" + Environment.NewLine +
                "[00:05:01.00] ••• team1 (player1) starts turn" + Environment.NewLine +
                "[00:06:02.00] ••• team1 (player1) fires Grenade" + Environment.NewLine +
                "[00:07:04.00] ••• team1 (player1) ends turn; time used: 11.58 sec turn, 3.00 sec retreat" + Environment.NewLine +
                "[00:08:04.22] ••• Damage dealt: 45 (1 kill) to team4 (player4)" + Environment.NewLine +
                "[00:05:01.00] ••• team2 (player2) starts turn" + Environment.NewLine +
                "[00:07:04.00] ••• team2 (player2) ends turn; time used: 11.58 sec turn, 3.00 sec retreat";

            var turns = _turnParser.ParseTurns(log);

            Assert.That(turns, Has.Length.EqualTo(3));
            Assert.That(turns[0].Team, Is.EqualTo("team6 (player6)"));
            Assert.That(turns[1].Team, Is.EqualTo("team1 (player1)"));
            Assert.That(turns[2].Team, Is.EqualTo("team2 (player2)"));
            Assert.That(turns[0].WeaponActions, Has.Length.EqualTo(2));
            Assert.That(turns[1].WeaponActions, Has.Length.EqualTo(1));
            Assert.That(turns[2].WeaponActions, Has.Length.EqualTo(0));
        }
    }
}
