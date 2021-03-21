using System;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Resources.Replays.Text;

namespace Worms.Armageddon.Resources.Tests.Replays
{
    public class ReplayTextReaderShould
    {
        private IReplayTextReader _replayTextReader;

        private const string RedTeam = "Red Team";
        private const string BlueTeam = "Blue Team";
        private const string GreenTeam = "Green Team";
        private const string YellowTeam = "1UP";
        private const string MagentaTeam = "Test Team";
        private const string CyanTeam = "Last Team Name";

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
                $"Red: \"{RedTeam}\"" + Environment.NewLine +
                $"Blue: \"{BlueTeam}\"" + Environment.NewLine +
                $"Green: \"{GreenTeam}\"" + Environment.NewLine +
                $"Yellow: \"{YellowTeam}\"" + Environment.NewLine +
                $"Magenta: \"{MagentaTeam}\"" + Environment.NewLine +
                $"Cyan: \"{CyanTeam}\"" + Environment.NewLine;

            var replay = _replayTextReader.GetModel(log);

            replay.Teams.Count.ShouldBe(6);
            replay.Teams.ShouldContain(RedTeam);
            replay.Teams.ShouldContain(BlueTeam);
            replay.Teams.ShouldContain(GreenTeam);
            replay.Teams.ShouldContain(YellowTeam);
            replay.Teams.ShouldContain(MagentaTeam);
            replay.Teams.ShouldContain(CyanTeam);
        }

        [Test]
        public void ReadTeamsForOnlineMatch()
        {
            var log =
                $"Red: \"Player1\"     as \"{RedTeam}\"" + Environment.NewLine +
                $"Blue: \"Player 2\"    as \"{BlueTeam}\"" + Environment.NewLine +
                $"Green: \"Player3\"   as \"{GreenTeam}\"" + Environment.NewLine +
                $"Yellow: \"PlayerFour\"  as \"{YellowTeam}\"" + Environment.NewLine +
                $"Magenta: \"Player Five\" as \"{MagentaTeam}\"" + Environment.NewLine +
                $"Cyan: \"Player 8\"    as \"{CyanTeam}\"" + Environment.NewLine;

            var replay = _replayTextReader.GetModel(log);

            replay.Teams.Count.ShouldBe(6);
            replay.Teams.ShouldContain(RedTeam);
            replay.Teams.ShouldContain(BlueTeam);
            replay.Teams.ShouldContain(GreenTeam);
            replay.Teams.ShouldContain(YellowTeam);
            replay.Teams.ShouldContain(MagentaTeam);
            replay.Teams.ShouldContain(CyanTeam);
        }

        [Test]
        public void ReadWinnerOfMatchFromReplay()
        {
            var log = $"{RedTeam} wins the match!";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe(RedTeam);
        }

        [Test]
        public void ReadWinnerOfRoundFromReplay()
        {
            var log = $"{RedTeam} wins the round.";

            var replay = _replayTextReader.GetModel(log);

            replay.Winner.ShouldBe(RedTeam);
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
