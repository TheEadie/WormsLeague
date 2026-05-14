# Plan: Parser Placement Extraction

## Context

This is the first slice of the ELO Rankings epic. It extends the `Worms.Armageddon.Files` replay model
to include a `Placements` collection on `ReplayResource`, where each entry associates a `Team` with an
integer finish position derived from worm kill events already present in the parsed log. No other slice
has run before this one; the only pre-existing data available is what `ReplayTextReader` and its
`IReplayLineParser` pipeline already parse into `ReplayResource` (teams, turns with per-team damage
and kill counts, winner string).

The calculation is a post-processing step over the complete, accumulated turn list rather than a
per-line parse action, so it is invoked inside `ReplayResourceBuilder.Build()` after all parsers have
run — not via a new `IReplayLineParser` implementation.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/Worms.Armageddon.Files/Replays/Placement.cs` | New `Placement` record type |
| `src/Worms.Armageddon.Files/Replays/PlacementCalculator.cs` | Internal sealed class that computes placements from turns + winner |
| `src/Worms.Armageddon.Files.Tests/Replays/PlacementCalculatorShould.cs` | NUnit + Shouldly unit tests for all acceptance criteria |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Armageddon.Files/Replays/ReplayResource.cs` | Add `IReadOnlyCollection<Placement> Placements` parameter to `ReplayResource` record |
| `src/Worms.Armageddon.Files/Replays/ReplayResourceBuilder.cs` | Call `PlacementCalculator.Calculate(_turns, _winner)` in `Build()` and pass result to `ReplayResource` |

---

## Implementation Details

### 1. New `Placement` record

**File:** `src/Worms.Armageddon.Files/Replays/Placement.cs`

```csharp
using JetBrains.Annotations;

namespace Worms.Armageddon.Files.Replays;

[PublicAPI]
public record Placement(Team Team, int Position);
```

`[PublicAPI]` is required: the record is consumed externally (the Hub Worker will read it in slice 2).
Use `public` visibility for the same reason.

---

### 2. Update `ReplayResource` record

**File:** `src/Worms.Armageddon.Files/Replays/ReplayResource.cs`

Add `IReadOnlyCollection<Placement> Placements` as a new positional parameter. Place it after `Turns`
and before `FullLog` (or at the end — order doesn't matter functionally; putting it after `Turns`
groups it with the turn-derived data):

```csharp
[PublicAPI]
public record ReplayResource(
    DateTime Date,
    bool Processed,
    IReadOnlyCollection<Team> Teams,
    string Winner,
    IReadOnlyCollection<Turn> Turns,
    IReadOnlyCollection<Placement> Placements,
    string FullLog);
```

No other files in `Worms.Armageddon.Files` construct `ReplayResource` directly other than
`ReplayResourceBuilder.Build()`, so the only code change required outside the new files is in
`ReplayResourceBuilder`.

The existing test class `ReplayTextReaderShould` never constructs `ReplayResource` directly — all
assertions go through `_replayTextReader.GetModel(log)` — so the record signature change does not
require modifying any existing test.

---

### 3. `PlacementCalculator` — algorithm

**File:** `src/Worms.Armageddon.Files/Replays/PlacementCalculator.cs`

```csharp
namespace Worms.Armageddon.Files.Replays;

internal static class PlacementCalculator
{
    public static IReadOnlyCollection<Placement> Calculate(
        IReadOnlyCollection<Turn> turns,
        IReadOnlyCollection<Team> teams,
        string winner)
    {
        // ...
    }
}
```

**Guard:** if `winner` is empty (no winner line parsed — abandoned/truncated log), return
`Array.Empty<Placement>()` immediately.

**Step A — infer worm count per team.**

Accumulate `WormsKilled` from each `Turn`'s `Damage` collection across all turns, per team.
Once a team's running total has reached the inferred worm count the excess is ignored (see Step B).
For step A, do a first pass ignoring the cap to find the raw maximum total kills for any single team;
that maximum is `wormsPerTeam`.

Actually, to handle the "excess kills are ignored" requirement correctly, the cap must be `wormsPerTeam`.
But `wormsPerTeam` is itself derived from the capped totals — which is circular only if excess kills
actually exceed it. The resolution: use the uncapped maximum across all teams as `wormsPerTeam` (the
spec says "it equals the maximum total kills accumulated by any single team across the whole game"). Do
a single uncapped pass first to find this maximum, then do the placement pass applying the cap.

```
// Pass 1: uncapped totals to find wormsPerTeam
Dictionary<Team, uint> uncappedTotals = ...
foreach turn, foreach damage entry: uncappedTotals[team] += WormsKilled
wormsPerTeam = uncappedTotals.Values.Max()   // or 0 if no kills at all
```

If `wormsPerTeam` is 0 (no kills recorded in any turn), return `Array.Empty<Placement>()` — there is
no meaningful placement data.

**Step B — find elimination turn index per team (capped).**

Walk turns in index order `0..N-1`. Maintain `Dictionary<Team, uint> cumulativeKills`. For each
`Damage` entry in `turn[i]`:

- If `cumulativeKills[team]` is already >= `wormsPerTeam`, skip this entry.
- Otherwise add `WormsKilled` to `cumulativeKills[team]`.
- If `cumulativeKills[team]` now >= `wormsPerTeam`, record `eliminationTurn[team] = i`.

After all turns: teams with no entry in `eliminationTurn` survived (were not eliminated).

**Step C — assign positions.**

A team with a higher `eliminationTurn` index was eliminated later, so it performed better.
A team that survived (no entry) performed best of all — except in a draw where nobody survives.

Define each team's "rank key" as:
- `int.MaxValue` if the team survived (not in `eliminationTurn`)
- `eliminationTurn[team]` otherwise

Position of team T = (count of teams whose rank key is strictly greater than T's rank key) + 1.

This handles all cases uniformly:
- Normal win: the surviving team has rank key `int.MaxValue`; it gets position 1. All eliminated teams
  rank below it.
- Draw: all teams are in `eliminationTurn`; the teams eliminated in the last turn share the highest
  elimination turn index among all teams, so they get position 1. Teams eliminated earlier get higher
  positions.
- Tied positions: two teams with the same rank key both get the same position. The next position
  skips (e.g. two teams tied at 2 means the next is 4), because position = count of teams strictly
  ahead + 1, which for the next-worse group counts both tied teams.

Build the result as `List<Placement>` in order matching `teams` (preserves original team order
from the log).

---

### 4. Update `ReplayResourceBuilder.Build()`

**File:** `src/Worms.Armageddon.Files/Replays/ReplayResourceBuilder.cs`

Change `Build()` to:

```csharp
public ReplayResource Build()
{
    var placements = PlacementCalculator.Calculate(_turns, _teams, _winner);
    return new ReplayResource(_start, true, _teams, _winner, _turns, placements, _fullLog);
}
```

No other changes to the builder are needed.

---

### 5. New test class `PlacementCalculatorShould`

**File:** `src/Worms.Armageddon.Files.Tests/Replays/PlacementCalculatorShould.cs`

Test via `IReplayTextReader.GetModel(log)` exactly as `ReplayTextReaderShould` does — construct the
service from `new ServiceCollection().AddWormsArmageddonFilesServices()`. The class is `internal sealed`
with `[Test]` methods; each test builds a minimal inline log string and asserts on `replay.Placements`.

Cover all acceptance criteria from `spec.md`:

| Test method | Log content | Expected `Placements` |
|---|---|---|
| `ReturnPlacementsForThreeTeamsEliminatedInDifferentTurns` | 3 teams; kills that eliminate each team in turn 1, 2, 3 respectively | Team3 at pos 1 (survivor), Team2 at pos 2, Team1 at pos 3 |
| `ReturnTiedPlacementForTwoTeamsEliminatedInSameTurn` | 3 teams; Team1 and Team2 each get their last kill in the same turn; Team3 survives | Team3 at pos 1, Team1 and Team2 both at pos 2 |
| `ReturnAllTeamsAtPositionOneForFullDrawInSameTurn` | 3 teams, draw; all 3 eliminated in the same final turn | All 3 at pos 1 |
| `ReturnCorrectPositionsForPartialDrawWhereOneTeamEliminatedEarlier` | 3 teams, draw; Team1 eliminated in turn 1, Team2 and Team3 eliminated in turn 2 | Team2 and Team3 at pos 1, Team1 at pos 3 |
| `ReturnEmptyPlacementsWhenNoWinnerLine` | A log with turns and kills but no winner/draw line | Empty `Placements` |
| `IgnoreKillsRecordedBeyondWormsPerTeamCount` | A log where kills are recorded against a team after their worm count is already reached | The extra kills do not create a new earlier elimination for any other team |

**Constructing test logs:** the inline log strings should match the format already used in
`ReplayTextReaderShould`. For kills, use the pattern:
```
[HH:mm:ss.ff] ••• <TeamName> starts turn
[HH:mm:ss.ff] ••• Damage dealt: 100 (1 kill) to <EliminatedTeam> (<machine>)
[HH:mm:ss.ff] ••• <TeamName> ends turn; time used: 10.00 sec turn, 3.00 sec retreat
```

Use a worm count of 1 (simplest case: 1 worm per team, 1 kill eliminates) for most tests to keep
logs short. For the multi-kill test (excess kills), use worm count 2 to demonstrate capping.

**Teams in test logs:** use online format `Red: "machine" as "Team Name"` consistently.

---

## Verification

1. Run `dotnet test src/Worms.Armageddon.Files.Tests` — all tests (existing + new) must pass.
2. Run `dotnet build src/Worms.Armageddon.Files --warnaserror` — zero warnings, zero errors.
3. Run `dotnet build src/Worms.Armageddon.Files.Tests --warnaserror` — zero warnings, zero errors.
4. Confirm the existing `ReplayTextReaderShould` tests continue to pass without modification (they
   will: none of them assert on `Placements`, and none construct `ReplayResource` directly).
5. For each test in `PlacementCalculatorShould`, verify `replay.Placements` contains exactly the
   entries specified in the test description (count, team identity, position value).
6. Build the wider solution (`make cli.build`) to confirm no downstream assembly is broken by the
   `ReplayResource` record change.
