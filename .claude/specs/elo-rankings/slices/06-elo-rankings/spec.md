# ELO Rankings

## Overview

Integrate the PlayerRank library to compute per-league ELO ratings for players, triggered whenever a replay is processed. Standings (player name, ELO, games played) are exposed on the existing league API endpoints and displayed in a new section above the replays table on the league detail page.

## Requirements

- The `PlayerRank` NuGet package is added as a dependency to `Worms.Hub.Gateway`.

- A DB migration adds a `player_ratings` table storing per-player per-league ELO ratings: `(player_auth_subject, league_id, rating, games_played)` with `(player_auth_subject, league_id)` as a unique key.

- An `IRatingsRepository` is added to `Worms.Hub.Storage` supporting: reading all ratings for a league (returning `player_auth_subject`, `display_name` joined from the players table, `rating`, and `games_played`), and replacing all ratings for a league (delete existing, insert fresh).

- A `RatingsCalculator` service is added to `Worms.Hub.Gateway` with the following behaviour:
  - Accepts a league ID.
  - Reads all processed replays for that league, ordered by date ascending (alphabetical by filename as tiebreaker).
  - For each replay, maps each placement's `(machine, team name)` to a player via the teams table; unmatched placements are excluded.
  - Replays where no team is matched to a player are skipped entirely (excluded from both ELO and games played).
  - Replays where exactly one team is matched to a player are excluded from the ELO calculation but still count toward that player's `games_played`.
  - Replays where two or more teams are matched to players are included in the ELO calculation via PlayerRank.
  - Builds a PlayerRank game result set from the matched placements only (skipping teams with no claimed alias).
  - Computes ELO ratings using `EloScoringStrategy` with a starting rating of 1000. `Points` wraps a `double` internally but ELO values are always whole numbers (the library rounds every delta with `MidpointRounding.AwayFromZero`); read them as `int` via `(int)playerScore.Points.GetValue()`.
  - Games played is the count of processed replays in the league where the player had at least one aliased team in the placements (including single-player replays excluded from ELO).
  - Only players with at least one game appear in the resulting ratings.
  - Writes the computed ratings back to the `player_ratings` table, replacing any existing ratings for that league.

- After the Worker Processor updates a replay and marks it `Processed`, it calls `RatingsCalculator` for the replay's league. If the calculation throws, the error is logged and replay processing continues — the replay is still marked `Processed` and the queue message is deleted.

- The existing `GET /api/v1/leagues` (list) and `GET /api/v1/leagues/{id}` (detail) endpoints are extended: the `LeagueDto` gains a `standings` field containing an array of `StandingDto` objects, each with `playerName` (string), `elo` (int), `gamesPlayed` (int). Results are ordered by ELO descending.

- `standings` is `null` in both endpoints when the `player_ratings` table does not yet exist (i.e. the migration has not been applied), gated via a new `IsEloRatingsEnabledAsync()` method on `IFeatureFlags` that checks the DB schema version against the V0.8 migration. When the table exists but no rated players are present for a league, `standings` is an empty array.

- The `LeagueDetailPage` in the Web UI is extended to show a standings table above the existing replays table. The table has columns: Rank, Player Name, ELO, Games Played. The standings section is silently omitted when `standings` is `null` or an empty array.

## Out of Scope

- ELO delta per game on placement pills on the game detail page (deferred — see plan.md).
- Top-3 ELO leaderboard preview on league list cards (deferred — see plan.md).
- ELO recalculation triggered by alias claim or unclaim (next slice: "ELO on alias changes").
- Global cross-league ratings.
- Ranking modes other than ELO.
- Backfill of historical data from external systems.

## Acceptance Criteria

- **PlayerRank dependency**: `PlayerRank` appears in `Worms.Hub.Gateway.csproj` and the project builds with `--warnaserror`.

- **DB migration**: the `player_ratings` table is created by a new Flyway migration in `src/database/migrations/`.

- **Standings in API — schema gate**: before the migration is applied, `GET /api/v1/leagues` and `GET /api/v1/leagues/{id}` return `"standings": null` and do not error.

- **Standings in API — rated players**: after the migration is applied and at least one replay has been processed with at least one team matched to a player, `GET /api/v1/leagues/{id}` returns a `standings` array containing one entry per player who appeared in at least one game; each entry has `playerName`, `elo`, and `gamesPlayed`; the array is ordered by `elo` descending.

- **Standings in API — no rated players**: for a league where no team aliases are claimed, `standings` is an empty array (not null).

- **Starting ELO**: a player who has played exactly one game starts from a baseline of 1000 before that game's delta is applied.

- **Partial aliases**: given a replay where team A is claimed by a player and team B is unclaimed, the ELO update includes only the player for team A; team B is excluded. The replay still counts toward the claiming player's games played.

- **Single-player replay**: given a replay where exactly one team is claimed by a player (all others unclaimed), the replay is excluded from the ELO calculation but still counts toward that player's `gamesPlayed`.

- **ELO recalculation on replay processed**: after a new replay is processed by the Worker, `GET /api/v1/leagues/{id}` reflects updated ELO values in the `standings` field.

- **ELO calc failure does not block processing**: if `RatingsCalculator` throws during replay processing, the replay is still marked `Processed`, the queue message is deleted, and the error appears in the logs.

- **League with no aliased teams**: when a league has processed replays but none of their placements match a claimed alias, `standings` is an empty array.

- **Standings section — shown**: when the league detail page loads and `standings` contains at least one entry, a standings table is rendered above the replays table showing Rank, Player Name, ELO, and Games Played.

- **Standings section — omitted**: when `standings` is `null` or an empty array, no standings section or placeholder is rendered on the league detail page.

- **Web build passes**: `make web.lint` passes after the UI changes.

## Open Questions

None.
