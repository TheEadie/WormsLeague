# Learnings: ELO Rankings

## Implementation Notes

### `PlayerRank.League` and `PlayerRank.Game` clash with `Worms.Hub.Storage.Domain.League` and `Game`

The plan's `RatingsCalculator` code used `new League()` and `new Game()` without qualification. The `Worms.Hub.Storage.Domain` namespace (pulled in via storage repository dependencies) also defines `League` and `Game` types, causing CS0104 ambiguous-reference errors at build time. The plan did not mention this conflict.

The fix was to keep `using PlayerRank;` for `Position` and `Points` (which have no conflicts), and to qualify only the two conflicting types as `new PlayerRank.League()` and `new PlayerRank.Game()`. An alias approach (`using PrLeague = PlayerRank.League`) would also work but fully-qualified names are clearer given the ambiguity is local to two constructor calls.

## Files Added (not in plan)

None — all created and modified files were listed in the plan's "Files to Create / Modify" table.
