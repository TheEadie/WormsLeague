# Review — Remove Feature Flags

## Verdict

The implementation cleanly removes the two schema-version-gated feature flags and the supporting `DatabaseSchemaVersion` infrastructure exactly as specified. All six call sites are inlined to the "enabled" branch, the DI registrations and stranded `using` directives are gone, the `LeagueDto.Standings` field is tightened to non-nullable in both the gateway DTO and the two Web UI pages, and both steering docs (`hub-gateway.md`, `hub-storage.md`) are rewritten so they no longer name the deleted types. `dotnet build src/Worms.Hub.Gateway --warnaserror` succeeds with 0 warnings / 0 errors; `npx eslint src` and `npx tsc -b` in the Web project both pass clean; `make cli.test.unit` is green. A repo-wide grep for `IFeatureFlags|GatewayFeatureFlags|IsTeamsEnabledAsync|IsEloRatingsEnabledAsync|DatabaseSchemaVersion` across `src/` and `.claude/docs/` returns zero matches. No blockers.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| No matches for the five flag/version identifiers anywhere in `src/` or `.claude/docs/` | MET | `grep -rE "IFeatureFlags\|GatewayFeatureFlags\|IsTeamsEnabledAsync\|IsEloRatingsEnabledAsync\|DatabaseSchemaVersion" src .claude/docs` returns nothing |
| `Worms.Hub.Gateway/FeatureFlags/` folder no longer exists | MET | Both `IFeatureFlags.cs` and `FeatureFlags.cs` shown as deleted in diff; directory is now empty/absent |
| `TeamsController.GetAll` always returns the team list | MET | `TeamsController.cs:16-21` — no flag gate, returns `Ok(...)` |
| `TeamsController.Put` always claims and always recalculates ratings | MET | `TeamsController.cs:24-63` — flag guards removed; `ratingsCalculator.CalculateForTeam(...)` unconditional at line 60 |
| `LeaguesController.GetAll` / `Get` always populate `Standings` from the repository | MET | `LeaguesController.cs:21-26` and `:49-52` — `standings` materialised unconditionally |
| `LeagueDto.Standings` and `FromDomain` parameter are non-nullable | MET | `LeagueDto.cs:7,14` — both typed `IReadOnlyList<StandingDto>` (no `?`) |
| Web `LeagueDto.standings` is non-nullable; guards collapse to `.length > 0` | MET | `LeagueListPage.tsx:23,81` and `LeagueDetailPage.tsx:29,165` |
| `Processor.UpdateReplay` always upserts teams and always runs ELO when `LeagueId is not null` | MET | `Processor.cs:86-94` (foreach unwrapped); `:96` (`if (updatedReplay.LeagueId is not null)` with try/catch retained) |
| `StartupBackfiller.BackfillRatings` no longer early-returns on a flag | MET | `StartupBackfiller.cs:75` — only `IConfiguration` resolved in the scope; flag block deleted |
| `hub-gateway.md` "Feature flags" section gone; "Deployment safety" no longer names deleted types | MET | Diff removes lines 69-71 of the section and rewrites the schema-version-gate bullet |
| `hub-storage.md` no longer references `DatabaseSchemaVersion` | MET | "Degrade gracefully" bullet rewritten in diff |
| `dotnet build --warnaserror` succeeds | MET | `dotnet build src/Worms.Hub.Gateway --warnaserror` → 0 Warning(s), 0 Error(s). Pre-existing `NU1902` on `Worms.Hub.Infrastructure` is unrelated to this slice (documented in learnings) |
| Unit tests pass | MET | `make cli.test.unit` — all 309 tests pass; no Gateway test project exists to mock `IFeatureFlags` |

## Scope

The diff touches exactly the files listed in the plan's "Files to Create / Modify" table:

- **Deleted (3 files, 1 dir):** `IFeatureFlags.cs`, `FeatureFlags.cs`, `DatabaseSchemaVersion.cs`, and the now-empty `FeatureFlags/` directory.
- **Modified (production):** the two `ServiceRegistration.cs` files, `TeamsController.cs`, `LeaguesController.cs`, `LeagueDto.cs`, `Processor.cs`, `StartupBackfiller.cs`, `LeagueListPage.tsx`, `LeagueDetailPage.tsx`.
- **Modified (docs):** `hub-gateway.md`, `hub-storage.md`, and epic `plan.md` (checkbox for this slice ticked).

No out-of-scope files appear in the diff. `learnings.md` calls out one near-miss (the `Microsoft.Extensions.DependencyInjection.Extensions` `using` had to be retained in Gateway `ServiceRegistration.cs` because `TryAddScoped<RatingsCalculator>()` still uses it) — the final diff shows that directive correctly preserved (no removal in the diff).

## Blockers

None.

## Suggestions

None.

## Nitpicks

None.

## Tests

No production test code was added or modified by this slice. That is correct given the scope: no test project mocked `IFeatureFlags` (confirmed by the zero-match grep across `src/` including test projects), so removing it required no test edits. The behavioural change is the *removal* of gates whose enabled branch was already the exercised path — existing integration-level coverage of teams listing, claim/unclaim with ratings recalculation, league standings, and the worker's replay-processing pipeline continues to exercise the same code paths. No coverage gaps introduced. No padding or fragile patterns to flag.

## Recommended Actions

No findings to action.
