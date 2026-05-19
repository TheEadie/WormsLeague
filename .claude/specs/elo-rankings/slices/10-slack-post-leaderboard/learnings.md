# Learnings: Slack Post — League Leaderboard with ELO Changes

## Implementation Notes

### Implementation was already complete at session start

All code changes described in plan.md were already present in the working tree
before this implement-slice session began. The session therefore covered only
build and test verification (step 5 of the plan) and the post-implementation
bookkeeping steps (steps 5 and 6 of the command).

### `CultureInfo.InvariantCulture` required for int-to-string formatting

The plan's code snippets used unqualified string interpolation for integer
formatting (e.g. `e.Rank.ToString().Length`, `$"{rank}: {elo} ..."`).
The actual implementation in `LeaderboardFormatter.cs` uses
`ToString(CultureInfo.InvariantCulture)` and
`$"{rank}: {elo} {safeName}{suffix}"` via
`sb.AppendLine(CultureInfo.InvariantCulture, ...)` throughout. This is required
to satisfy the `CA1305` ("Specify IFormatProvider") warning that `--warnaserror`
promotes to an error. Not called out in the plan; consistent with the repo's
general pattern for numeric formatting.

### `List<LeaderboardEntry>?` in Processor, `IReadOnlyList<LeaderboardEntry>?` on the interface

The plan specifies `IReadOnlyList<LeaderboardEntry>? leaderboard` throughout.
In `Processor.UpdateReplay`, the local variable is declared as
`List<LeaderboardEntry>? leaderboard = null` (the mutable concrete type), which
is then passed where `IReadOnlyList<LeaderboardEntry>?` is expected. This is
valid C# (implicit upcast) and avoids an unnecessary cast. Not a deviation from
the spec, just a detail the plan did not spell out.

### Build verifies clean: zero warnings, all 309 unit tests pass

`dotnet build --warnaserror src/Worms.Hub.Gateway` produced 0 warnings, 0
errors. `make cli.test.unit` ran 309 tests across four test assemblies; all
passed.

## Files Added (not in plan)

None — all created and modified files were listed in the plan's
"Files to Create / Modify" table.
