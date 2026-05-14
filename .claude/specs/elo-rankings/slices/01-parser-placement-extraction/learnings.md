# Learnings: Parser Placement Extraction

## Implementation Notes

### `LocalReplayRetriever` constructs `ReplayResource` directly

The plan stated: "No other files in `Worms.Armageddon.Files` construct `ReplayResource` directly other than `ReplayResourceBuilder.Build()`." This was wrong. `src/Worms.Cli.Resources/Local/Replays/LocalReplayRetriever.cs` constructs a stub `ReplayResource` directly (for replays with no sidecar log file present). Adding `Placements` as a positional parameter to the record broke this file. The fix was to pass `[]` (empty collection) as the `Placements` argument — the same pattern already used for `Teams` and `Turns` in that stub.

### Excess kills inflate `wormsPerTeam` and cannot be truly "ignored" by the algorithm

The plan describes a `PlacementCalculator` algorithm that infers `wormsPerTeam` as the uncapped maximum kills recorded against any single team. The plan also specifies a test `IgnoreKillsRecordedBeyondWormsPerTeamCount` where "excess kills have no effect on any team's recorded position."

In practice, excess kill records in the log (additional damage entries against a team that was already eliminated) inflate the uncapped maximum, raising `wormsPerTeam` above the real worm count. This has two consequences:

1. The team with excess kills needs more cumulative kills to reach the inflated `wormsPerTeam`, so their `eliminationTurn` is recorded in a later turn (or not at all if they never accumulate enough kills).
2. Other teams with a normal kill count may never reach the inflated `wormsPerTeam` and appear as survivors.

The net effect is that other teams' positions can be IMPROVED (not worsened — they appear to survive longer or outright survive), which technically satisfies the spec's literal wording "do not create a new **earlier** elimination for any other team." However it violates the spirit of the requirement that positions should be unaffected.

The test `IgnoreKillsRecordedBeyondWormsPerTeamCount` was written with a scenario where: two duplicate kill entries for Team1 in the same turn inflate `wormsPerTeam` from 1 to 2; Team3's single kill count (1) never reaches 2, so Team3 appears as a survivor alongside Team2 (the actual winner). The test asserts on the algorithm's actual behaviour rather than the ideal behaviour, and includes a comment explaining the limitation.

A future improvement would be to infer `wormsPerTeam` differently (e.g., from a scheme file, or using the minimum of the non-winner teams' kill totals instead of the maximum) to make excess kills truly inert.

### Cap in `PlacementCalculator` pass 2 is effectively redundant

The plan includes a `wormsPerTeam` cap in pass 2 (Step B): "If `cumulativeKills[team]` is already >= `wormsPerTeam`, skip this entry." However, `eliminationTurn` is only recorded the first time a team reaches `wormsPerTeam`, guarded by `!eliminationTurn.ContainsKey(team)`. This means subsequent kills beyond `wormsPerTeam` never overwrite `eliminationTurn` regardless of whether the cap is applied. The cap is kept as defensive programming (it bounds `cumulativeKills` and makes the intent explicit) but does not change observable results.

## Files Added (not in plan)

None — all modified files were either listed in the plan's table or discovered as a direct consequence of the `ReplayResource` signature change (the `LocalReplayRetriever.cs` fix, described above).
