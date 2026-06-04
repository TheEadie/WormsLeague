using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Files.Replays.Text;

namespace Worms.Armageddon.Files.Tests.Replays;

internal sealed class PlacementCalculatorShould
{
    private readonly IReplayTextReader _replayTextReader;

    public PlacementCalculatorShould()
    {
        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonFilesServices();
        var serviceProvider = services.BuildServiceProvider();
        _replayTextReader = serviceProvider.GetRequiredService<IReplayTextReader>();
    }

    [Test]
    public void ReturnAllTeamsAtPositionOneForDraw()
    {
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:20.00] ••• Team1 (machine1) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           The round was drawn.
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
    }

    [Test]
    public void ReturnNullPositionsWhenNoWinnerLine()
    {
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team2 (machine2)
                           [00:01:20.00] ••• Team1 (machine1) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(2);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == null);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == null);
    }

    [Test]
    public void ReturnSoloWinnerAtPositionOne()
    {
        const string log = """
                           Red: "machine1" as "Team1"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:20.00] ••• Team1 (machine1) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           Team1 wins the match!
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 1);
    }

    [Test]
    public void ReturnAllTeamsAtPositionOneWhenWinnerNameMatchesNoTeam()
    {
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:20.00] ••• Team1 (machine1) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           Ghost wins the match!
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
    }

    [Test]
    public void ReturnWinnerAtPositionOneAndOthersAtPositionTwo()
    {
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:20.00] ••• Team1 (machine1) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           Team3 wins the match!
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 2);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 2);
    }
}
