# Review — Placement Persistence

## Verdict

The implementation satisfies every acceptance criterion in the spec. Both `Worms.Hub.Storage` and `Worms.Hub.Gateway` build clean with `--warnaserror` and `make cli.build`. The transactional write, V04/V05 repository split, backfill service, and feature-flag gating are all correct. One suggestion and two nitpicks are raised — none block merging.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| After migration applied and a replay is processed, `replay_placements` contains one row per team with correct `machine`, `team_name`, and `position` | MET | `Processor.cs:75-78` maps `replayModel.Placements` → `ReplayPlacement`; `ReplaysRepositoryV05.cs:119-129` INSERTs per placement inside the transaction |
| Two teams eliminated on the same turn share the same `position` value | MET | Position value comes from the parser's output unchanged; no de-duplication or override; `ReplaysRepositoryV05.cs:126` stores `p.Position` as-is |
| Worker deployed against schema < V0.5 completes without error and writes no placement rows | MET | `ServiceRegistration.cs:23-25` selects `ReplaysRepositoryV04` for schema < V0.5; `ReplaysRepositoryV04.Update()` references no `replay_placements` table |
| Worker starts against schema ≥ V0.5 with empty `replay_placements`, placements computed from `full_log` for each `Processed` replay | MET | `PlacementsBackfillService.cs:29-66` — feature-flag check, count check, then iterates `GetAll().Where(r => r.Status == "Processed")` |
| Worker starts against schema ≥ V0.5 with rows already in `replay_placements`, backfill is skipped | MET | `PlacementsBackfillService.cs:38-41` — `COUNT(*) > 0` guard returns early |
| Existing `Processed` replay with null `full_log` is skipped without error | MET | `PlacementsBackfillService.cs:48-51` — `string.IsNullOrEmpty(replay.FullLog)` skips with debug log |
| Placement write fails mid-transaction → replay row update also rolled back, queue message left in queue | MET | `ReplaysRepositoryV05.Update()` uses a single `BeginTransaction()` covering both UPDATE and INSERT; `Processor.cs:79-85` calls `DeleteMessage` only after `Update()` returns; uncaught exception propagates up without deleting the message |
| `GetAll()` and `GetByLeagueId()` on V0.5 repository return `Replay` objects with correct `Placements` list (empty list for replays with no placement rows) | MET | `ReplaysRepositoryV05.cs:35-38` uses `.ToLookup()` — missing keys return empty enumerable → empty `List<ReplayPlacement>`; not null |
| `GetAll()` and `GetByLeagueId()` on V0.4 repository return `Replay` objects with `Placements = null` | MET | `ReplayDb.ToDomain()` (`ReplayDb.cs:31`) now passes `null` as final argument; both V04 read methods call `x.ToDomain()` unchanged |

## Scope

All files listed in the plan's "Files to Create / Modify" table are present in the diff and working tree. No files were added outside the plan. No planned files are missing.

The `deployment/Worms.Hub.Infrastructure/Pulumi.yaml` database version (`0.4.1`) is not updated to `0.5` — this follows the established repo pattern of bumping the infrastructure version in a separate PR (see commits `b4f3f3cf`, `e1b1ab39`). Not a deviation.

## Blockers

None.

## Suggestions

#### S1 — Extract duplicate `int.Parse` in `ReplaysRepositoryV05.Update()`

- **File:** `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs:101,112`
- **Issue:** `int.Parse(item.Id, CultureInfo.InvariantCulture)` is called twice — once inline in the anonymous object for the UPDATE statement and again to populate `replayId` for the DELETE/INSERT. The two results are identical.
- **Fix:** Declare `var id = int.Parse(item.Id, CultureInfo.InvariantCulture)` before the `Execute` call and use `id` (or rename to `replayId` throughout) in both places.
- **Decision:** Accept

## Nitpicks

#### N1 — `Transaction.Rollback()` is implicit but not explicit

- **File:** `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs:93`
- **Issue:** `using var transaction = connection.BeginTransaction()` relies on Npgsql's `Dispose()` to roll back if the method throws before `Commit()`. This is correct and idiomatic for Npgsql, but a future reader may not know this. The rest of the codebase has no other transactional writes to compare against.
- **Fix:** No code change required — just a note. Could add a brief comment above the `using` block: `// transaction rolls back automatically on Dispose if Commit() is not reached`. Accept only if the team values explicitness here.
- **Decision:** Decline

#### N2 — `ILogger` implicit using without an explicit `using` directive

- **File:** `src/Worms.Hub.Gateway/Worker/PlacementsBackfillService.cs:1-11`
- **Issue:** `ILogger<PlacementsBackfillService>` and `IServiceProvider` are referenced without an explicit `using Microsoft.Extensions.Logging;` directive. They resolve via the ASP.NET Core implicit usings. This is standard for `Microsoft.NET.Sdk.Web` projects but differs from the explicit-using style used in `Worms.Hub.Storage` files. Not a build issue — just an inconsistency within the Gateway project's own files (e.g. `Processor.cs` also relies on implicit usings for `ILogger`).
- **Fix:** No action needed — consistent with the rest of the Gateway project.
- **Decision:** Decline

## Tests

No new test projects were added. The plan explicitly calls this out: correctness is verified by `make cli.build` (which passes) and manual stack verification. This is consistent with the testing strategy doc, which notes that hub gateway and storage do not currently have dedicated unit-test projects, and that database behaviour is best covered by integration tests against real Postgres. No coverage gap exists relative to what the spec asked for.

The backfill logic (feature-flag check, count guard, per-replay try/catch, null log skip) is the most meaningful new logic introduced. The testing strategy doc indicates that adding a `<Project>.Tests` is preferred when meaningful logic is added at these layers — however, the logic is thin and the plan explicitly decided against new test projects for this slice. No padding tests were added.

## Recommended Actions

- **S1** — Accept — Extracting the parse to a single variable removes duplication and makes the relationship between the UPDATE's `@id` parameter and the DELETE/INSERT's `@replayId` clearer.
- **N1** — Decline — Npgsql's rollback-on-dispose behaviour is well-documented and the comment would add noise without changing correctness.
- **N2** — Decline — The implicit-using pattern is consistent with the rest of the Gateway project and does not affect correctness or maintainability.
