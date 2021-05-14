using System;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Resources.Replays;
using Worms.Armageddon.Resources.Replays.Text;

namespace Worms.Armageddon.Resources.Tests.Replays
{
    public class ReplayTextReaderShould
    {
        private IReplayTextReader _replayTextReader;

        private readonly Team _redTeam = new("Red Team", "local", "Red");
        private readonly Team _blueTeam = new("Blue Team", "local", "Blue");
        private readonly Team _greenTeam = new("Green Team", "local", "Green");
        private readonly Team _yellowTeam = new("1UP", "local", "Yellow");
        private readonly Team _magentaTeam = new("Test Team", "local", "Magenta");
        private readonly Team _cyanTeam = new("Last Team Name", "local", "Cyan");

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
    }
}
