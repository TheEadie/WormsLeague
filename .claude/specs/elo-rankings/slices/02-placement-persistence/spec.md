# Placement Persistence

## Overview

Introduce a `replay_placements` table and extend the Hub Storage and Worker so that finish positions for all teams in a replay are persisted atomically with the replay update. A startup backfill computes placements from the stored `full_log` of all previously-processed replays.

## Requirements

- A new `replay_placements` table stores one row per team per replay, with columns for `replay_id` (FK to `replays`), `machine`, `team_name`, and `position` (integer, 1-based; two rows may share the same position value to represent a tie). The primary key is `(replay_id, machine, team_name)`.
- A Flyway migration at version V0.5 creates the `replay_placements` table.
- A new `ReplayPlacement` record is added to the `Worms.Hub.Storage` domain, carrying `machine`, `team_name`, and `position`.
- The `Replay` domain record gains an `IReadOnlyList<ReplayPlacement>?` field. Null means placements are unavailable (old schema); an empty list means the replay was processed but no placement data was determined.
- `IReplaysRepository.Update()` writes the replay row and all placement rows in a single database transaction. Callers pass a `Replay` with placements populated; the repository implementation handles transactionality internally. `Create()` ignores the `Placements` field on both V04 and V05 — placements are not written at creation time.
- The `ReplaysRepositoryV04` implementation (schema < V0.5) ignores the placements field on `Update()` and returns `null` for placements on all read methods.
- A `ReplaysRepositoryV05` implementation (schema ≥ V0.5) is a standalone class following the same pattern as `ReplaysRepositoryV04` — no inheritance. It writes placement rows atomically with the replay `UPDATE`: within the same transaction it deletes all existing `replay_placements` rows for that `replay_id` then inserts the new set. This makes the write idempotent — safe on queue retry. It also populates placements on `GetAll()` and `GetByLeagueId()`.
- The `ServiceRegistration` DI factory selects `ReplaysRepositoryV05` when the detected schema version is ≥ V0.5, and `ReplaysRepositoryV04` otherwise.
- The Gateway Worker (`Processor`) maps `replayModel.Placements` to `IReadOnlyList<ReplayPlacement>` and includes it on the `Replay` passed to `replayRepository.Update()`. No feature-flag check is needed in the live processing path — passing placements unconditionally is safe because `ReplaysRepositoryV04` discards them silently.
- `IFeatureFlags` gains an `IsPlacementsEnabledAsync()` method, backed by a schema version ≥ V0.5 check inside `GatewayFeatureFlags`. The startup backfill is gated through this flag; the live `Update()` path is not.
- A dedicated `IHostedService` (`PlacementsBackfillService`) runs once at Worker startup. When `IFeatureFlags.IsPlacementsEnabledAsync()` returns true and the `replay_placements` table is empty, it iterates all replays with `Status = "Processed"`: for each, it parses `full_log` via `IReplayTextReader`, computes placements, and calls `Update()`. Replays with a null `full_log` are skipped silently. If `Update()` throws for an individual replay, the error is logged and the backfill continues with the next replay. If any rows exist in `replay_placements` the service exits immediately on the assumption the backfill has already run.
- Each backfill write uses the same transactional `Update()` path as live processing.
- If the placement write fails during live processing (transaction rolls back), the Worker must not delete the queue message — the error propagates so the message becomes visible again for retry.

## Out of Scope

- Displaying placements in the CLI, Web UI, or Slack (covered by "Placement display").
- Using `IsPlacementsEnabledAsync()` to gate display of placement data in the CLI, Web UI, or Slack (that is covered by "Placement display").
- Any schema migration for tables beyond `replay_placements` (aliases, players, ELO).
- Backfilling replays that have no stored `full_log`.

## Acceptance Criteria

- Given the migration has been applied and a replay is processed by the Worker, the `replay_placements` table contains one row per team in the replay with the correct `machine`, `team_name`, and `position`.
- Given two teams are eliminated on the same turn, both rows share the same `position` value.
- Given the Worker is deployed against a schema older than V0.5, replay processing completes without error and no placement rows are written.
- Given the Worker starts against a schema ≥ V0.5 and the `replay_placements` table is empty, placements are computed from `full_log` and written for each `Processed` replay.
- Given the Worker starts against a schema ≥ V0.5 and the `replay_placements` table already contains rows, the backfill is skipped.
- Given an existing `Processed` replay has a null `full_log`, the backfill skips it without error.
- Given a placement write fails mid-transaction, the replay row update is also rolled back and the queue message is left in the queue.
- Given `GetAll()` or `GetByLeagueId()` is called on the V0.5 repository, the returned `Replay` objects include the correct `Placements` list (including an empty list for replays with no placement rows).
- Given `GetAll()` or `GetByLeagueId()` is called on the V0.4 repository, the `Placements` field on all returned `Replay` objects is null.

## Open Questions

None.
