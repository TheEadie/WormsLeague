# Learnings: ELO Delta on Game Detail

## Implementation Notes

### Roslynator RCS1077 flags `Select(...).ToList()` on a `List<T>` as suboptimal

The plan's pseudocode built the `MultiPlayerSelection` list with
`matchedPlayers.Select(mp => new MultiPlayerSelection(...)).ToList()`. Because
`matchedPlayers` is already a `List<T>` (the result of an earlier `.ToList()`),
Roslynator's RCS1077 ("Optimize LINQ method call") flagged this as an error
under `--warnaserror`, requiring `matchedPlayers.ConvertAll(mp => new
MultiPlayerSelection(...))`. The plan did not call this out; previous slices
in this epic typically chain LINQ directly on `IEnumerable<T>`, so the rule
did not surface earlier. Worth remembering when materialising a projection
into a `List<T>` after an earlier `.ToList()`.

### `PlayerRank.History` lives in `PlayerRank.Scoring`

`league.GetLeaderBoardHistory(...)` returns `IEnumerable<History>` where the
`History` type sits in the `PlayerRank.Scoring` namespace, not the top-level
`PlayerRank` namespace. The plan's helper signature used `History?` without
specifying the using directive; `using PlayerRank.Scoring;` had to be added
alongside the existing `using PlayerRank;` and `using PlayerRank.Scoring.Elo;`.
Not a blocker, just one more using line than the plan suggested.

## Files Added (not in plan)

None — all created and modified files were listed in the plan's
"Files to Create / Modify" table.

## Deferred — Calculator unit tests

Spec criterion (line 101) calls for `RatingsCalculator` unit tests covering the
multi-player, single-matched-player, multi-team-same-player, unclaimed, and
first-game cases. This slice does **not** add them. Rationale:

- The gateway has no `Worms.Hub.Gateway.Tests` project today and slice 06
  (`08-elo-leaderboard-on-league-cards`) made the same deferral when adding
  comparable orchestration logic. Introducing the first gateway test project
  inside this slice would balloon scope.
- `testing-strategy.md` notes the gateway lacks a unit-test project and
  recommends adding one "when adding meaningful logic at those layers" — the
  delta computation qualifies, so this is a real gap, not a no-op.
- Verification for this slice relied on integration via `docker compose up`
  (plan §Verification steps 4–8) and the existing `--warnaserror` build gate.

**Action:** revisit in the epic retro — establishing `Worms.Hub.Gateway.Tests`
with coverage of `RatingsCalculator`'s five enumerated cases should be picked
up as a follow-up slice or retro action item, not silently dropped.
