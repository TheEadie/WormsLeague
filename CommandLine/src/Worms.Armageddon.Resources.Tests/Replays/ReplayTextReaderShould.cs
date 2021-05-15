using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Resources.Replays;
using Worms.Armageddon.Resources.Replays.Text;

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
            _replayTextReader = new ReplayTextReader();
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
        public void ReadWinnerOfMatchFromReplay()
        {
            var log = $"{_redTeam.Name} wins the match!";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe(_redTeam.Name);
        }

        [Test]
        public void ReadWinnerOfRoundFromReplay()
        {
            var log = $"{_redTeam.Name} wins the round.";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe(_redTeam.Name);
        }

        [Test]
        public void ReadWinnerAsADrawFromReplay()
        {
            const string log = "The round was drawn.";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe("Draw");
        }

        [Test]
        public void ReadTurnFromOnlineReplay()
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
        public void ReadTurnFromOfflineReplay()
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

    }
}
