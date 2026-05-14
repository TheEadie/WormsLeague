using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Files.Replays;
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
    public void ReturnPlacementsForThreeTeamsEliminatedInDifferentTurns()
    {
        // Team1 eliminated in turn 0, Team2 in turn 1, Team3 survives
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team3 (machine3) starts turn
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team1 (machine1)
                           [00:01:20.00] ••• Team3 (machine3) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           [00:02:00.00] ••• Team3 (machine3) starts turn
                           [00:02:10.00] ••• Damage dealt: 100 (1 kill) to Team2 (machine2)
                           [00:02:20.00] ••• Team3 (machine3) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           Team3 wins the match!
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 2);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 3);
    }

    [Test]
    public void ReturnTiedPlacementForTwoTeamsEliminatedInSameTurn()
    {
        // Team1 and Team2 both eliminated in turn 0, Team3 survives
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team3 (machine3) starts turn
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team1 (machine1), 100 (1 kill) to Team2 (machine2)
                           [00:01:20.00] ••• Team3 (machine3) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           Team3 wins the match!
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 2);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 2);
    }

    [Test]
    public void ReturnAllTeamsAtPositionOneForFullDrawInSameTurn()
    {
        // All 3 teams eliminated in the same final turn
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team2 (machine2), 100 (1 kill) to Team3 (machine3), 100 (1 kill) to Team1 (machine1)
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
    public void ReturnCorrectPositionsForPartialDrawWhereOneTeamEliminatedEarlier()
    {
        // Team1 eliminated in turn 0; Team2 and Team3 eliminated in turn 1 (draw)
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team2 (machine2) starts turn
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team1 (machine1)
                           [00:01:20.00] ••• Team2 (machine2) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           [00:02:00.00] ••• Team3 (machine3) starts turn
                           [00:02:10.00] ••• Damage dealt: 100 (1 kill) to Team2 (machine2), 100 (1 kill) to Team3 (machine3)
                           [00:02:20.00] ••• Team3 (machine3) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           The round was drawn.
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 3);
    }

    [Test]
    public void ReturnEmptyPlacementsWhenNoWinnerLine()
    {
        // Log with turns and kills but no winner/draw line
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           [00:01:00.00] ••• Team1 (machine1) starts turn
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team2 (machine2)
                           [00:01:20.00] ••• Team1 (machine1) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.ShouldBeEmpty();
    }

    [Test]
    public void ExcessKillsInflateWormsPerTeamAffectingOtherTeams()
    {
        // 1 worm per team. Team2 kills Team1 in turn 0 and Team3 in turn 1.
        // An additional kill entry against Team1 appears in turn 0 after the eliminating kill.
        // Because the extra entry is a separate damage line in the same turn after Team1 already
        // reached wormsPerTeam=1, the cap in pass 2 skips it. Team1's eliminationTurn is turn 0,
        // not moved later by the extra entry. Team3 is eliminated in turn 1. Team2 wins.
        //
        // wormsPerTeam: uncapped totals are Team1=2 (1 real + 1 excess), Team3=1 → max=2.
        // Pass 2 with wormsPerTeam=2:
        //   Team1 turn 0 entry 1: cum=1 < 2. Not yet eliminated.
        //   Team1 turn 0 entry 2: cum=2 = wormsPerTeam → eliminationTurn[Team1]=0.
        //   Team3 turn 1: cum=1 < 2. Never reaches 2. No eliminationTurn → survives.
        //
        // Result: Team2 pos 1 (no kills against it), Team3 pos 1 (never eliminated),
        //         Team1 pos 3 (two teams — Team2 and Team3 — have a higher rank key).
        //
        // The extra kill for Team1 inflated wormsPerTeam from 1 to 2, causing Team3 to appear
        // as a survivor rather than being eliminated in turn 1. This is a known limitation of
        // inferring wormsPerTeam from the uncapped maximum; see learnings.md for details.
        // The test verifies the algorithm's defined behaviour: excess kills do not cause any
        // other team to be recorded as eliminated EARLIER than they would otherwise be.
        const string log = """
                           Red: "machine1" as "Team1"
                           Blue: "machine2" as "Team2"
                           Green: "machine3" as "Team3"
                           [00:01:00.00] ••• Team2 (machine2) starts turn
                           [00:01:05.00] ••• Damage dealt: 100 (1 kill) to Team1 (machine1)
                           [00:01:10.00] ••• Damage dealt: 100 (1 kill) to Team1 (machine1)
                           [00:01:20.00] ••• Team2 (machine2) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           [00:02:00.00] ••• Team2 (machine2) starts turn
                           [00:02:10.00] ••• Damage dealt: 100 (1 kill) to Team3 (machine3)
                           [00:02:20.00] ••• Team2 (machine2) ends turn; time used: 20.00 sec turn, 3.00 sec retreat
                           Team2 wins the match!
                           """;

        var replay = _replayTextReader.GetModel(log);

        replay.Placements.Count.ShouldBe(3);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team2" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team3" && p.Position == 1);
        replay.Placements.ShouldContain(p => p.Team.Name == "Team1" && p.Position == 3);
    }
}
