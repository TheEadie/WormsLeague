# Review — ELO on Alias Changes

## Verdict

The implementation satisfies all acceptance criteria. Both `dotnet build --warnaserror` targets exit clean with zero warnings, and the full unit test suite passes. One pre-existing violation of the component doc's `TryAddScoped` convention is now made more visible by this slice — it is not new, but worth fixing. No blockers.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| ELO feature enabled + claim: `player_ratings` reflects alias across all historical replays | MET | `TeamsController.cs:72-75` — `CalculateForTeam` is called after `SetPlayerClaim` on the claim branch, which runs `Calculate(leagueId)` for every affected league |
| ELO feature enabled + unclaim: `player_ratings` no longer includes contributions from alias | MET | `TeamsController.cs:69` sets the claim to null, then the same `CalculateForTeam` block at line 72 reruns all affected leagues |
| Team in leagues L1 and L2: both recalculated before response | MET | `RatingsCalculator.cs:83-95` iterates all league IDs from `GetAffectedLeagueIds` before returning; `TeamsController.cs:72-75` awaits the feature flag then calls synchronously |
| Team in no replays: claim/unclaim returns 200, no recalculation attempted | MET | `ReplaysRepositoryV05.cs:112-124` returns an empty collection when no matching placements exist; `foreach` body is never entered |
| ELO feature flag not enabled: returns 200, no recalculation | MET | `TeamsController.cs:72` guards the call with `await featureFlags.IsEloRatingsEnabledAsync()` |
| L1 throws, L2 still runs, claim/unclaim returns 200 | MET | `RatingsCalculator.cs:86-94` wraps `Calculate(leagueId)` in try/catch; `CA1031` suppressed with specific justification; `logger.LogError` at line 92 |
| Existing 409 Conflict and 403 Forbidden responses unaffected | MET | `TeamsController.cs:48-50` returns `Conflict()` and `TeamsController.cs:64-66` returns `Forbid()` before reaching the ELO block |

## Scope

All five files listed in the plan's "Files to Create / Modify" table are present in the diff. No files outside the plan appear in the diff (the `.claude/specs/elo-rankings/plan.md` diff is a workflow artefact and is excluded per the review rules).

Both deviations from the plan are documented in `learnings.md`:

- **Roslynator RCS1124**: plan used a local variable `var leagueIds = ...`; implementation inlines it directly into `foreach`. Correctly resolved.
- **Expression-bodied to block-body**: `AddGatewayServices` was a single expression-bodied method; converted to block body to accommodate the `TryAddScoped` call. Correctly resolved.

## Blockers

None.

## Suggestions

#### S1 — `AddWorkerServices` should use `TryAddScoped<RatingsCalculator>()` for monolith-safe registration

- **File:** `src/Worms.Hub.Gateway/ServiceRegistration.cs:36`
- **Issue:** `AddWorkerServices` still registers `RatingsCalculator` via `AddScoped<RatingsCalculator>()`. In monolith mode `AddGatewayServices` runs first (line 50 of `Program.cs`) and registers via `TryAddScoped`; then `AddWorkerServices` runs and adds a second registration via `AddScoped`. The component doc (`hub-gateway.md`) states: "any method registered from multiple components must use `TryAddScoped` / `TryAddSingleton` / `TryAddEnumerable` throughout so repeated calls in monolith mode are safe". The double registration is functionally harmless (same concrete type, same resolved instance) but violates the stated convention.
- **Fix:** Change `.AddScoped<RatingsCalculator>()` to `.TryAddScoped<RatingsCalculator>()` (and move it outside the fluent chain, as done in `AddGatewayServices`, since `TryAddScoped` is void).
- **Decision:** Accept

## Nitpicks

None.

## Tests

No new tests were added. This is consistent with the plan's rationale (section 6): the slice adds pure orchestration wiring — method delegation and DI — with no standalone logic that a unit test can usefully exercise in isolation without mocking every dependency. The plan explicitly notes that if a dedicated gateway test project is added in future, `CalculateForTeam` is simple enough to cover at that point. No coverage gap is introduced beyond the pre-existing absence of a gateway unit-test project.

## Recommended Actions

- **S1** — Accept — The fix is a one-line change that brings `AddWorkerServices` into compliance with the component doc's `TryAddScoped` convention; it eliminates a silent double-registration in monolith mode.
