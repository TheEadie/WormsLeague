# ELO Delta on Game Detail

## Overview

Show each player's per-game ELO change and resulting rating on the placement pills of the game/replay detail page. Deltas are computed by the existing `RatingsCalculator` during its normal rebuild, persisted alongside placements, and exposed through the replay API.

## Requirements

- A DB migration adds two new nullable columns to `replay_placements`: `elo_delta INTEGER NULL` and `elo_after INTEGER NULL`.

- `RatingsCalculator.Calculate(leagueId)` is extended so that, in addition to writing the league standings as today, it also writes per-placement `elo_delta` and `elo_after` values for every replay in the league. The per-game ELO bookkeeping is delegated to `PlayerRank`; the calculator only has to map snapshots back to placements and handle the cases PlayerRank does not model.
  - As the calculator iterates replays in the existing date-then-name order, it records, per replay, the index of the corresponding game in PlayerRank's stream (i.e. the count of `RecordGame` calls so far at the point that replay's game is recorded). Replays where `RecordGame` is not invoked (< 2 matched players) have no recorded-game index.
  - After the existing loop, the calculator calls `league.GetLeaderBoardHistory(eloStrategy)` once. This returns one snapshot per recorded game, preceded by an initial snapshot at index `0` in which every player who ever plays is implicitly at `1000`. The snapshot after recorded-game `g` is therefore `history[g]`.
  - For each replay with a recorded-game index `g`, each matched player's selected placement gets `elo_after = (int)history[g].Leaderboard[player].Points.GetValue()` and `elo_delta = elo_after − (int)history[g - 1].Leaderboard[player].Points.GetValue()`. Because `history[0]` is `1000` for every player who ever plays, a player's first game naturally satisfies `elo_after − elo_delta === 1000` without any special-case code.
  - For replays with exactly one matched player (`RecordGame` not invoked), that single placement is written with `elo_delta = 0` and `elo_after` equal to that player's rating in the most recent snapshot before this replay in iteration order (i.e. `history[gPrev]` where `gPrev` is the count of `RecordGame` calls made before reaching this replay). If `gPrev == 0` or the player is absent from that snapshot (e.g. they have never appeared in a multi-player replay yet), `elo_after = 1000`.
  - When a player has two or more claimed teams in the same replay, only the placement that `RatingsCalculator` already uses (best position) receives non-null `elo_delta`/`elo_after`. The calculator must remember the specific `placementId` it selected per (replay, player) so values are written back to that exact row. The player's other placements in that replay keep `elo_delta` and `elo_after` as `NULL`.
  - Placements for unclaimed teams keep `elo_delta` and `elo_after` as `NULL`.
  - Placements for replays excluded entirely from the calculation (no matched players, or skipped because the placement has no position) keep `elo_delta` and `elo_after` as `NULL`.
  - `RatingsCalculator.Calculate` overwrites `elo_delta`/`elo_after` for **every** placement on **every** replay in the league in a single update pass — each row receives either the freshly-computed integer or `NULL`. No separate "clear" step. End-state invariant: no stale values from a previous run remain.

- `StartupBackfiller` is extended so its ratings backfill also fires when at least one league has placements that *should* carry delta data but don't. The detection query targets placements meeting all of: claimed team, position present, replay `Status = 'Processed'`, replay has ≥ 2 matched players — if any such placement has `elo_after IS NULL`, that league needs backfilling. The backfill mechanism remains a single `RatingsCalculator.Calculate(leagueId)` call per league.

- The `IPlacementsRepository` / `IReplaysRepository` (whichever currently reads placement rows) returns the new fields on the domain model, and the `RatingsRepository` (or the calculator) writes them. The placement write API is keyed by `placementId` so the calculator can target the exact row it selected for each (replay, player).

- The `PlacementDto` returned by `GET /api/v1/leagues/{id}/replays/{replayId}` and the list endpoint `GET /api/v1/leagues/{id}/replays` is extended with two new fields:
  - `eloDelta: number | null`
  - `eloAfter: number | null`
  Both are sourced directly from the new columns; no extra computation in the API layer.

- The web `PlacementDto` interface (in `PlacementPill.tsx`) gains the same two fields.

- `PlacementPill` renders an ELO badge segment when `eloAfter !== null`:
  - The badge sits inside the pill, separated from the player/team text by a vertical divider (matching the design mockup layout).
  - The top line shows `eloAfter` in the existing `monoFontFamily`, bold, font size 13, primary text colour.
  - The bottom line shows the delta in the same mono font, bold, font size 10, with sign-aware colour:
    - `eloDelta > 0` → `success.main`, prefixed with `+` (e.g. `+12 ELO`).
    - `eloDelta < 0` → `error.main` (e.g. `-8 ELO`).
    - `eloDelta === 0` → `text.disabled` (`0 ELO`).
  - On the winner's pill (`isWin === true`), the same colour rules apply except they are read against the warning-tinted background; no special override is required beyond keeping the existing pill styling.

- When `eloAfter` is `null`, no badge segment and no divider are rendered — the pill renders exactly as it does today.

- The change to `PlacementDto` (server and web) is additive only; existing fields and their meaning are unchanged.

## Out of Scope

- Any change to the ELO algorithm, starting rating, K-factor, or PlayerRank configuration.
- Showing ELO deltas anywhere other than placement pills on the game/replay detail page (no changes to the league standings table, league cards, CLI output, or Slack messages).
- Showing a player's ELO history or trend over time.
- A separate trigger or job for delta recalculation — deltas are always recomputed as part of the existing `RatingsCalculator.Calculate` invocation (replay-processed, alias claim, alias unclaim, startup backfill).
- Surfacing deltas before `StartupBackfiller` has populated them — `elo_delta`/`elo_after` simply remain `NULL` and the UI omits the badge.
- Per-player drill-through from a pill.
- Changes to game placement extraction, alias claiming, or replay processing.

## Acceptance Criteria

- **Migration applied**: a new Flyway migration in `src/database/migrations/` adds nullable `elo_delta` and `elo_after` columns to `replay_placements`; existing rows have both columns set to `NULL`.

- **Calculator writes deltas — multi-player replay**: given a league with two processed replays each containing two matched players, after `RatingsCalculator.Calculate` runs, each matched placement has non-null `elo_delta` and `elo_after`; the sum of `elo_delta` values across the two players in a single replay is `0` (zero-sum ELO).

- **Calculator writes deltas — N-player zero-sum**: for any processed replay with N ≥ 2 matched players, the sum of `elo_delta` across those N players is `0` (tolerating ±1 integer rounding).

- **Calculator writes deltas — first game**: for a player's first game in a league with at least two matched players, `elo_after − elo_delta === 1000`.

- **Calculator writes deltas — single-matched-player replay**: when a processed replay has exactly one matched player, that player's placement has `elo_delta === 0` and `elo_after` equal to that player's `lastKnownRating` at the point this replay is reached in date order (or `1000` if they have no prior multi-player game). The single-matched replay does **not** advance `lastKnownRating`. No other placement in that replay receives non-null values.

- **Calculator writes deltas — unclaimed teams**: placements for teams with no claimed alias have `elo_delta === NULL` and `elo_after === NULL` after recalculation.

- **Calculator writes deltas — player with multiple teams in one replay**: when one player has two claimed teams in a replay, exactly one of those two placements (the one with the best position, matching the existing `RatingsCalculator` selection) has non-null `elo_delta`/`elo_after`; the other has both as `NULL`.

- **Recalc leaves no stale values**: after `RatingsCalculator.Calculate` runs for a league, every placement row across every replay in that league reflects the latest computation (either a freshly-written integer or `NULL`); no stale `elo_delta`/`elo_after` values from a previous run remain. Achieved via a single overwrite pass, not a separate clear step.

- **Recalc on replay processed**: after a new replay is processed, placements for the new replay and any other affected replays have updated `elo_delta`/`elo_after` values reflecting the new game history.

- **Recalc on alias claim/unclaim**: claiming or unclaiming an alias for a team triggers full recalculation that updates `elo_delta`/`elo_after` across all affected replays' placements, consistent with the existing standings recalculation.

- **Startup backfill — fresh installation**: when the `player_ratings` table is empty, `StartupBackfiller` runs `RatingsCalculator.Calculate` for each league, populating both standings and per-placement deltas.

- **Startup backfill — pre-slice data**: when the `player_ratings` table is already populated but at least one league has a placement matching (claimed team + position present + replay `Status = 'Processed'` + replay has ≥ 2 matched players) with `elo_after IS NULL`, `StartupBackfiller` runs `RatingsCalculator.Calculate` for those leagues, populating the new columns.

- **Startup backfill — no-op**: when no league has any placement matching the above detection query with `elo_after IS NULL`, `StartupBackfiller` does not re-run the calculator. (Intentionally-NULL placements — unclaimed teams, single-matched replays' bystanders, non-best multi-team rows — do not trigger backfill.)

- **API exposes new fields — detail**: `GET /api/v1/leagues/{id}/replays/{replayId}` returns each placement with `eloDelta` and `eloAfter` fields populated from the database (either an integer or `null`).

- **API exposes new fields — list**: `GET /api/v1/leagues/{id}/replays` returns the same `PlacementDto` shape (including the new fields) for each replay's placements.

- **Post-migration, pre-backfill**: after the V0.9 migration has applied but before `StartupBackfiller` has run, all placement rows have `elo_delta`/`elo_after` as `NULL`, the API serialises both fields as `null`, and the UI omits the badge segment on every pill.

- **UI — badge rendered**: on the game detail page, when `eloAfter` is non-null on a placement, the pill renders an additional segment (separated by a left divider) showing `eloAfter` on top and `eloDelta` formatted as `{sign}{value} ELO` below.

- **UI — badge omitted**: when `eloAfter` is `null` on a placement, the pill renders exactly as it does today — no divider, no badge, no placeholder.

- **UI — delta colour**: positive deltas render in `success.main`, negative in `error.main`, zero in `text.disabled`. The post-game rating renders in `text.primary`.

- **UI — winner pill**: a winning placement with non-null delta still renders the badge inside the warning-tinted pill; the colour rules above apply to the delta text.

- **Existing pill content unchanged**: position circle, player/team text, and the Claim button continue to render in their current positions and styles for both badged and unbadged pills.

- **Build and lint**: `make web.lint`, `make web.build`, and `dotnet build --warnaserror` all pass.

- **Tests**: unit tests cover `RatingsCalculator` writing deltas for the multi-player, single-matched-player, multi-team-same-player, unclaimed, and first-game cases; existing tests continue to pass.

## Open Questions

None.
