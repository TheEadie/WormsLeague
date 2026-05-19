# Plan: Remove Feature Flags

## Context

This is the final slice of the ELO Rankings epic. Slices 1–10 introduced teams, placements, alias claiming, ELO ratings, and the Slack leaderboard post — all gated behind two schema-version-driven feature flags (`IFeatureFlags.IsTeamsEnabledAsync`, `IsEloRatingsEnabledAsync`) so gateway and worker pods could be safely deployed ahead of (or alongside) the database migrations. The feature is fully shipped to production and the gates are now dead code. This slice removes both flag predicates, the `IFeatureFlags` / `GatewayFeatureFlags` types, the `DatabaseSchemaVersion` infrastructure they relied on, and the DI registrations and `using` directives that go with them. As a consequence of the ELO flag going away, `LeagueDto.Standings` is always populated, so it is tightened to non-nullable in both the gateway DTO and the Web UI's local TypeScript shape. The steering docs that previously documented these patterns are rewritten to keep the underlying guidance (independent gateway/DB rollout is a real risk) without naming deleted types.

## Files to Create / Modify

### New files

None.

### Deleted files

| Path | Reason |
|---|---|
| `src/Worms.Hub.Gateway/FeatureFlags/IFeatureFlags.cs` | Flag interface no longer used |
| `src/Worms.Hub.Gateway/FeatureFlags/FeatureFlags.cs` | `GatewayFeatureFlags` implementation no longer used |
| `src/Worms.Hub.Gateway/FeatureFlags/` (directory) | Empty after the two files above are deleted |
| `src/Worms.Hub.Storage/Database/DatabaseSchemaVersion.cs` | Only consumer was `GatewayFeatureFlags` |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Gateway/ServiceRegistration.cs` | Drop both `IFeatureFlags` registrations and the `using Worms.Hub.Gateway.FeatureFlags;` directive |
| `src/Worms.Hub.Storage/ServiceRegistration.cs` | Drop the `AddSingleton<DatabaseSchemaVersion>()` registration |
| `src/Worms.Hub.Gateway/API/Controllers/TeamsController.cs` | Remove `IFeatureFlags` ctor param, drop the two teams gates and the one ELO gate, remove the `using` |
| `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` | Remove `IFeatureFlags` ctor param, always populate `standings`, pass non-null list to `LeagueDto.FromDomain`, remove the `using` |
| `src/Worms.Hub.Gateway/API/DTOs/LeagueDto.cs` | Tighten `Standings` and the `standings` parameter to non-nullable |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Remove `IFeatureFlags` ctor param, drop the teams gate and the ELO gate (retaining the ELO try/catch + `LeagueId is not null` guard), remove the `using` |
| `src/Worms.Hub.Gateway/Worker/StartupBackfiller.cs` | Drop the ELO-enabled early return and its scope-resolved `IFeatureFlags`, remove the `using` |
| `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx` | Type `standings: StandingDto[]`; collapse null guard |
| `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` | Type `standings: StandingDto[]`; collapse null guard |
| `.claude/docs/components/hub-gateway.md` | Delete the "Feature flags" section; rewrite the "Schema-version gate" bullet in "Deployment safety" so it no longer names `DatabaseSchemaVersion` or `IFeatureFlags` |
| `.claude/docs/components/hub-storage.md` | Rewrite the "Degrade gracefully" bullet in "Schema compatibility" so it no longer names `DatabaseSchemaVersion` |

---

## Implementation Details

### 1. Delete the `FeatureFlags` folder and `DatabaseSchemaVersion`

Delete the files. `IFeatureFlags.cs` and `FeatureFlags.cs` are the only contents of `src/Worms.Hub.Gateway/FeatureFlags/`, so the directory itself can be removed afterwards. `DatabaseSchemaVersion.cs` has no other in-repo consumers (verified by grep across `src/` and `.claude/docs/`).

### 2. `ServiceRegistration.cs` (Hub Gateway)

In `AddGatewayServices()`, the current fluent chain ends with `.AddScoped<IFeatureFlags, GatewayFeatureFlags>();`. Drop that call so the chain terminates after `.AddScoped<CliFileValidator>()`.

In `AddWorkerServices()`, drop the line `builder.TryAddScoped<IFeatureFlags, GatewayFeatureFlags>();`. The other `TryAddScoped<RatingsCalculator>()` line stays — it is still required because both methods can run in monolith mode.

Remove the `using Worms.Hub.Gateway.FeatureFlags;` directive at the top of the file.

### 3. `ServiceRegistration.cs` (Hub Storage)

The chain currently begins:
```
builder.AddSingleton<DatabaseSchemaVersion>()
    .AddScoped<IRepository<Game>, GamesRepository>()
```
Drop the `AddSingleton<DatabaseSchemaVersion>()` call so the chain begins with `builder.AddScoped<IRepository<Game>, GamesRepository>()`. No `using` directive needs removal — `DatabaseSchemaVersion` lives in `Worms.Hub.Storage.Database`, which is already imported for the other repositories.

### 4. `TeamsController.cs`

Remove `IFeatureFlags featureFlags,` from the primary constructor parameter list.

In `GetAll`: delete the entire `if (!await featureFlags.IsTeamsEnabledAsync()) { return NotFound(); }` block (lines 20–23 in the current file). The method body then starts directly with `var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);`. Because there is no remaining `await` on the flag, the method may no longer need `async` — but it still constructs the response and the method is declared `async Task<ActionResult<...>>`. **Keep the `async` modifier and return statement structure.** Wait — confirm: after removing the only `await`, the method has no `await` left. The compiler will warn (CS1998) under `--warnaserror`. Therefore: change the method signature from `async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll()` to `ActionResult<IReadOnlyList<TeamDto>> GetAll()` and wrap the result in `Ok(...)` directly (no `Task.FromResult` needed; ASP.NET Core accepts a synchronous controller action).

In `Put`: delete the second `if (!await featureFlags.IsTeamsEnabledAsync()) { return NotFound(); }` block (lines 33–36). Also delete the `if (await featureFlags.IsEloRatingsEnabledAsync())` wrapping (lines 72–75) so that `ratingsCalculator.CalculateForTeam(team.Machine, team.TeamName);` always runs unconditionally. `Put` still has other `await`s (none currently — review). Re-check: in the current `Put`, the only `await`s are the two `featureFlags` calls. After removal, `Put` has no `await`s. Change its signature from `async Task<ActionResult> Put(ClaimTeamDto body)` to `ActionResult Put(ClaimTeamDto body)` to avoid CS1998.

Remove the `using Worms.Hub.Gateway.FeatureFlags;` directive.

### 5. `LeaguesController.cs`

Remove `IFeatureFlags featureFlags` from the primary constructor parameter list (and the trailing comma above it from `IRatingsRepository ratingsRepository,`).

In `GetAll`: delete `var eloEnabled = await featureFlags.IsEloRatingsEnabledAsync();`. Replace the block:

```csharp
IReadOnlyList<StandingDto>? standings = null;
if (eloEnabled)
{
    standings = ratingsRepository.GetByLeagueId(dbLeague.Id)
        .Select(r => new StandingDto(r.DisplayName, r.Rating, r.GamesPlayed))
        .ToList();
}
```

with:

```csharp
IReadOnlyList<StandingDto> standings = ratingsRepository.GetByLeagueId(dbLeague.Id)
    .Select(r => new StandingDto(r.DisplayName, r.Rating, r.GamesPlayed))
    .ToList();
```

In `Get`: apply the analogous transformation — drop the `if (await featureFlags.IsEloRatingsEnabledAsync())` wrapping and always assign the materialised list. After this, `Get` has no remaining `await`s; the rest of the method still uses `await schemeFiles.GetLatestDetails(id)`, so `async` remains required.

`GetAll` keeps `async` because of `await schemeFiles.GetLatestDetails(...)` inside the `Select`.

Remove the `using Worms.Hub.Gateway.FeatureFlags;` directive.

### 6. `LeagueDto.cs`

Change the record declaration from:
```csharp
internal sealed record LeagueDto(string Id, string Name, Version? Version, Uri? SchemeUrl, IReadOnlyList<StandingDto>? Standings)
```
to:
```csharp
internal sealed record LeagueDto(string Id, string Name, Version? Version, Uri? SchemeUrl, IReadOnlyList<StandingDto> Standings)
```

Tighten the `standings` parameter of `FromDomain` from `IReadOnlyList<StandingDto>?` to `IReadOnlyList<StandingDto>`. The two call sites in `LeaguesController` are both updated (above) to pass a non-null list.

### 7. `Processor.cs`

Remove `IFeatureFlags featureFlags,` from the primary constructor parameter list.

Replace:
```csharp
if (await featureFlags.IsTeamsEnabledAsync())
{
    foreach (var placement in replayModel.Placements)
    {
        teamsRepository.Upsert(placement.Team.Machine, placement.Team.Name);
    }
}
```
with:
```csharp
foreach (var placement in replayModel.Placements)
{
    teamsRepository.Upsert(placement.Team.Machine, placement.Team.Name);
}
```

Replace the ELO gate:
```csharp
if (await featureFlags.IsEloRatingsEnabledAsync() && updatedReplay.LeagueId is not null)
```
with:
```csharp
if (updatedReplay.LeagueId is not null)
```
The surrounding try/catch and `leaderboard`/`leaderboardFailureNote` logic are retained verbatim.

Remove the `using Worms.Hub.Gateway.FeatureFlags;` directive. The `[SuppressMessage("Design", "CA1031", ...)]` attribute on `UpdateReplay` stays — the try/catch is still present.

### 8. `StartupBackfiller.cs`

In `BackfillRatings`, delete:
```csharp
var featureFlags = scope.ServiceProvider.GetRequiredService<IFeatureFlags>();

if (!await featureFlags.IsEloRatingsEnabledAsync())
{
    logger.LogInformation("ELO ratings feature not yet enabled — skipping ratings backfill.");
    return;
}
```

The remainder of the method (`connectionString` resolution onward) is preserved. The existing slice-9 detection short-circuit (`leaguesNeedingRecalc.Count == 0`) keeps warm prod databases a no-op.

After removal, `BackfillRatings` has no `await` directly inside its body until `connection.QuerySingleAsync<long>(...)` later — that one still exists, so `async` stays required.

Remove the `using Worms.Hub.Gateway.FeatureFlags;` directive.

### 9. Web UI: `LeagueListPage.tsx`

Change line 23 from:
```ts
standings: StandingDto[] | null
```
to:
```ts
standings: StandingDto[]
```

Change the guard on line 81 from:
```tsx
{league.standings !== null && league.standings.length > 0 && (
```
to:
```tsx
{league.standings.length > 0 && (
```

### 10. Web UI: `LeagueDetailPage.tsx`

Same two edits as above, at lines 29 and 165 respectively.

### 11. Steering docs — `hub-gateway.md`

Delete the entire "Feature flags" section (the heading on line 69 and the paragraph beneath it on line 71). This removes the prescription that "Controllers must depend on `IFeatureFlags`".

In the "Deployment safety" section, rewrite the bullet list so it no longer names the deleted types. Replace:
```markdown
- **Schema-version gate:** check `DatabaseSchemaVersion` (via `IFeatureFlags`) before querying the new table and return a suitable fallback or 503.
- **Feature flag:** hide the endpoint until the migration has been confirmed applied.
```
with:
```markdown
- **Schema-version gate:** detect the database's current schema version at request time and return a suitable fallback or 503 if the new column or table is not yet present. The ELO Rankings epic used a dedicated abstraction for this; if a similar need recurs, re-introduce a focused abstraction rather than inlining a version check into a controller.
- **Feature flag:** hide the endpoint until the migration has been confirmed applied.
```

The surrounding paragraph ("Any new endpoint backed by a DB migration must address independent gateway/DB rollout risk…") and the closing "This decision must be made during spec" line both stay.

### 12. Steering docs — `hub-storage.md`

Rewrite the "Degrade gracefully" bullet from:
```markdown
- **Degrade gracefully:** gate the new endpoint or query behind a `DatabaseSchemaVersion` check. The DI-factory pattern selects between a base repository implementation and a versioned subtype at startup based on the detected schema version, so controllers never reference `DatabaseSchemaVersion` directly.
```
to:
```markdown
- **Degrade gracefully:** gate the new endpoint or query behind a runtime schema-version check, returning a fallback (e.g. empty list, 503) until the migration has run. A DI-factory pattern that picks between a base repository and a versioned subtype at startup keeps the version check out of controller code.
```

The "Schema compatibility" heading, the introductory sentence, the "Require DB upgrade first" bullet, and the closing paragraph about enumerating write paths all stay unchanged.

### 13. Final sweep

After the edits above, re-run `grep -r "IFeatureFlags\|GatewayFeatureFlags\|IsTeamsEnabledAsync\|IsEloRatingsEnabledAsync\|DatabaseSchemaVersion"` across `src/` and `.claude/docs/` to confirm no stray references remain. Per spec acceptance criterion 1, this must return zero matches.

---

## Verification

1. `find src/Worms.Hub.Gateway/FeatureFlags -type f` returns nothing; the directory does not exist.
2. `find src/Worms.Hub.Storage/Database/DatabaseSchemaVersion.cs` returns nothing.
3. `grep -rE "IFeatureFlags|GatewayFeatureFlags|IsTeamsEnabledAsync|IsEloRatingsEnabledAsync|DatabaseSchemaVersion" src .claude/docs` returns zero matches.
4. `dotnet build --warnaserror` succeeds for the full solution. Pay particular attention to CS1998 (async method without `await`) warnings on the now-synchronous `TeamsController.GetAll` and `TeamsController.Put`.
5. `make cli.test.unit` passes (filters out integration tests).
6. The gateway test project(s) under `src/` for Gateway/Worker pass: `dotnet test src/Worms.Hub.Gateway.Tests` (or whatever test projects exist; run all non-integration tests). All existing tests pass without modification — no test currently mocks `IFeatureFlags` (verified during planning by absence of `IFeatureFlags` matches outside `/src/`).
7. The Web UI typechecks: `npm --prefix src/Worms.Hub.Web run build` (or the project's typecheck command) succeeds with the tightened `StandingDto[]` shape.
8. Manual smoke test (optional, against `docker compose up`): `GET /api/v1.0/leagues` and `GET /api/v1.0/leagues/{id}` both return `standings` as a JSON array (possibly empty), never `null`. `GET /api/v1.0/teams` returns a 200 with the teams list (no 404). A replay processed end-to-end still triggers ELO recalculation and Slack leaderboard post.
