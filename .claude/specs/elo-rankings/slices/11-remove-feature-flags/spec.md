# Remove Feature Flags

## Overview

Remove the two schema-version-gated feature flags (`IsTeamsEnabledAsync`, `IsEloRatingsEnabledAsync`) introduced during the ELO Rankings epic. The feature is fully deployed to production, so the gates are no longer needed. The supporting `DatabaseSchemaVersion` infrastructure becomes dead code once the flags are gone and is removed in the same slice.

The steering docs that document the `IFeatureFlags` / `DatabaseSchemaVersion` pattern are also updated in this slice, otherwise the next slice that adds a schema-gated feature will be guided toward a type that no longer exists. The `LeagueDto.Standings` field — previously nullable purely because of the ELO flag — is tightened to non-nullable in this slice, with the Web UI null-branch removed at the same time, so the type more honestly reflects what the gateway now always returns.

## Requirements

- The `IFeatureFlags` interface and its `GatewayFeatureFlags` implementation are removed from the Hub Gateway, along with the `Worms.Hub.Gateway/FeatureFlags/` folder.
- The `DatabaseSchemaVersion` class in `Worms.Hub.Storage` is removed.
- The DI registrations for `IFeatureFlags` are removed from **both** registration sites: `AddGatewayServices()` (`AddScoped`) and `AddWorkerServices()` (`TryAddScoped`, kept as `Try*` because both methods run in monolith mode) in `src/Worms.Hub.Gateway/ServiceRegistration.cs`.
- The DI registration for `DatabaseSchemaVersion` is removed from `src/Worms.Hub.Storage/ServiceRegistration.cs`.
- All six call sites that consume `IFeatureFlags` are updated to inline the "enabled" branch: the conditional and the disabled branch are deleted; only the code that previously ran when the flag was on remains. The six sites are: `TeamsController.GetAll`, `TeamsController.Put` (one teams-gate site and one ELO-gate site, both removed independently), `LeaguesController.GetAll`, `LeaguesController.Get`, `Processor.UpdateReplay` (one teams-gate site and one ELO-gate site, both removed independently), and `StartupBackfiller.BackfillRatings`.
- The now-stranded `using Worms.Hub.Gateway.FeatureFlags;` directives are removed from each modified file: `TeamsController.cs`, `LeaguesController.cs`, `Processor.cs`, `StartupBackfiller.cs`, and `ServiceRegistration.cs`. `--warnaserror` would also catch these, but they are called out explicitly to avoid surprise during implementation.
- `LeagueDto.Standings` is changed from `IReadOnlyList<StandingDto>?` to `IReadOnlyList<StandingDto>`. The `standings` parameter on `LeagueDto.FromDomain` is tightened to non-nullable in lockstep. Both `LeaguesController.GetAll` and `LeaguesController.Get` are updated to pass a list directly (no `null` initialiser).
- The Web UI `LeagueDto` interfaces in `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx` and `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` are tightened to `standings: StandingDto[]`, and the `league.standings !== null && …` guards collapse to `league.standings.length > 0`.
- The steering docs are updated in this slice:
  - `.claude/docs/components/hub-gateway.md`: the "Feature flags" section (currently documenting `IFeatureFlags` / `GatewayFeatureFlags`) is removed. The "Deployment safety" section's "Schema-version gate" bullet (which references `DatabaseSchemaVersion` via `IFeatureFlags`) is rewritten so it no longer names the deleted types but still conveys the underlying guidance: a new endpoint backed by a migration must still address independent gateway/DB rollout risk; the historical mitigation used a schema-version check, and any future need should re-introduce a similar abstraction rather than inlining it into a controller.
  - `.claude/docs/components/hub-storage.md`: the "Schema compatibility" section's "Degrade gracefully" bullet (which references `DatabaseSchemaVersion`) is rewritten the same way — keep the guidance that a slice adding columns must include an explicit compatibility decision, but stop naming the deleted type as the recommended mechanism.
- The project continues to build with `--warnaserror` and all existing tests continue to pass.

## Out of Scope

- Any infrastructure, deployment, environment-variable, or Docker Compose changes (verified to contain no references to the flags).
- Database migrations (no schema change is required).
- CLI changes: `LeagueDtoV1` in `src/Worms.Cli.Resources/Remote/WormsServerDtos.cs` does not include a `standings` field, so the gateway's tightened DTO has no CLI impact.
- Any change to the *behaviour* of teams, ELO ratings, or backfill beyond what removing the gate implies: the previously-gated code paths simply run unconditionally.
- Refactoring or restructuring of the six call-site methods beyond removing the conditional wrapping and the consequential `Standings` non-nullability changes.
- Adding a new feature-flagging mechanism for future use.

## Acceptance Criteria

- Given the codebase after this slice, when searching for `IFeatureFlags`, `GatewayFeatureFlags`, `IsTeamsEnabledAsync`, `IsEloRatingsEnabledAsync`, or `DatabaseSchemaVersion`, then no matches are found anywhere in the repository (production source, tests, or docs under `.claude/docs/`).
- Given the `Worms.Hub.Gateway/FeatureFlags/` folder, then it no longer exists.
- Given `TeamsController.GetAll`, when called, then it always returns the list of teams (no flag-based `NotFound` early return).
- Given `TeamsController.Put`, when a valid claim/unclaim request is received, then it always performs the claim and always triggers `RatingsCalculator.CalculateForTeam` (no flag-based `NotFound` early return, no flag-based skip of ratings recalculation).
- Given `LeaguesController.GetAll` and `LeaguesController.Get`, when called, then the returned `LeagueDto` always has `Standings` populated from `IRatingsRepository.GetByLeagueId` (possibly an empty list, but never `null`).
- Given the `LeagueDto` record, then its `Standings` property is typed `IReadOnlyList<StandingDto>` (non-nullable), and the `LeagueDto.FromDomain` factory takes a non-nullable `standings` parameter.
- Given the Web UI pages `LeagueListPage.tsx` and `LeagueDetailPage.tsx`, then the local `LeagueDto` interface declares `standings: StandingDto[]` (non-nullable), and the rendering guards are reduced to `league.standings.length > 0`.
- Given the worker `Processor.UpdateReplay`, when a replay is processed, then teams are always upserted from the parsed placements, and ELO is always calculated whenever `updatedReplay.LeagueId` is not null (the existing try/catch around the ELO calculation is retained).
- Given the worker `StartupBackfiller.BackfillRatings`, when the worker starts, then it always proceeds with the backfill (no flag-based early return). The existing slice-9 detection query (`leaguesNeedingRecalc.Count == 0` short-circuit) means this remains a no-op on warm databases that have already been backfilled, so removing the gate does not trigger spurious recomputation in production.
- Given the steering doc `.claude/docs/components/hub-gateway.md`, then the "Feature flags" section is gone and the "Deployment safety" mitigation list no longer references `DatabaseSchemaVersion` or `IFeatureFlags`.
- Given the steering doc `.claude/docs/components/hub-storage.md`, then the "Schema compatibility" section no longer references `DatabaseSchemaVersion`.
- Given the solution, when `dotnet build --warnaserror` is run, then it succeeds.
- Given the solution, when `make cli.test.unit` and the gateway/worker test suites are run, then all tests pass.

## Open Questions

None.
