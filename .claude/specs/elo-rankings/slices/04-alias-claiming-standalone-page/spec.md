# Alias Claiming — Standalone Page

## Overview

Introduce the players and teams tables, expose `GET /teams` and `PUT /teams/{id}` API endpoints, and deliver a standalone `/teams` Web UI page where authenticated users can claim or unclaim `(machine, team name)` pairs as their own identities.

## Requirements

- **DB migration V0.7** adds two tables:
  - `players`: `id` (integer PK), `auth0_subject` (text, unique, not null), `display_name` (text, not null)
  - `teams`: `id` (integer PK), `machine` (text, not null), `team_name` (text, not null), `player_id` (integer, nullable FK to players), UNIQUE(`machine`, `team_name`)
  - The migration backfills `teams` from all existing `replay_placements` rows via `INSERT INTO teams (machine, team_name) SELECT DISTINCT machine, team_name FROM replay_placements ON CONFLICT DO NOTHING`

- **Hub Storage** gains repositories for `Player` and `Team` domain records, following the existing `IRepository<T>` / Dapper pattern; the `Team` domain record carries `id`, `machine`, `teamName`, and optional `claimedByPlayerName`.

- **Hub Worker** is extended so that when placement data is written for a new replay, each `(machine, team_name)` pair is upserted into the `teams` table (INSERT … ON CONFLICT DO NOTHING), ensuring the teams list stays up to date as new replays are processed.

- **`GET /teams`** endpoint requires authentication and returns all rows from the `teams` table, globally across all leagues. Each item includes: `id`, `machine`, `teamName`, `claimedBy` (the `display_name` of the owning player, or `null` if unclaimed), and `isMyTeam` (true if the authenticated caller owns the team, false otherwise). The endpoint is gated behind schema version V0.7 via `IFeatureFlags`; if the migration has not been applied it returns 404.

- **`PUT /teams/{id}`** endpoint accepts a body `{ "claimed": true | false }` and returns an empty 200 on success. Behaviour:
  - `claimed: true` — if the team is unclaimed, creates a player record for the caller if none exists (Auth0 subject ID from the JWT, display name resolved as `nickname ?? name ?? sub` from the OIDC profile), then sets `player_id` to that player. If a player record already exists for the caller the display name is not updated. Returns 409 if already claimed by a different player.
  - `claimed: false` — clears `player_id` on the team. Returns 403 if the team is claimed by a different player. No-ops silently if already unclaimed.
  - Returns 404 if `{id}` does not exist.
  - Gated behind schema version V0.7 via `IFeatureFlags`; if the migration has not been applied it returns 404.

- **Web UI `/teams` page** (auth-required, not added to the header nav in this slice):
  - Displays all teams in a single combined list sorted as follows: unclaimed rows first, then rows where `isMyTeam` is true, then rows claimed by other players. Within each group rows are sorted alphabetically by machine name, then by team name.
  - Unclaimed rows show a **Claim** button.
  - Rows where `isMyTeam` is true show an **Unclaim** button.
  - Rows claimed by a different player show "Claimed by {name}" with no action.
  - Buttons are disabled while the request is in flight; on failure an inline error message is shown on the row and the row's state reverts. Error messages are specific to the HTTP status: 409 → "Already claimed by another player", 403 → "You don't own this team", any other failure → "Something went wrong, please try again".
  - When the list is empty, shows a descriptive message: "No teams found. Teams appear here once replays have been processed."

## Out of Scope

- Adding `/teams` to the header navigation (deferred to a later slice or to the ELO slice when the page becomes fully useful)
- ELO recalculation triggered by claiming or unclaiming (next slice: "ELO on alias changes")
- Any mention of ELO or standings on the `/teams` page
- Filtering or scoping teams by league
- Pagination of the teams list
- Admin ability to unclaim on behalf of another player
- Displaying which league a team was seen in
- Refreshing a player's display name after initial creation (deferred; a future slice will sync display names on login)

## Acceptance Criteria

- **Migration**: after applying V0.7, `players` and `teams` tables exist; `teams` is pre-populated with all distinct `(machine, team_name)` pairs from `replay_placements`.

- **Worker extension**: when a new replay is processed, any `(machine, team_name)` pair not already in `teams` is inserted; existing rows are left unchanged.

- **GET /teams — populated**: given processed replays exist, `GET /api/v1/teams` returns a list of objects with `id`, `machine`, `teamName`, `claimedBy` (null or a player display name), and `isMyTeam` (bool).

- **GET /teams — empty**: given no replays have been processed, `GET /api/v1/teams` returns an empty list with HTTP 200.

- **GET /teams — schema not applied**: given schema is below V0.7, `GET /api/v1/teams` returns HTTP 404.

- **PUT /teams/{id} claim — success**: given an unclaimed team, authenticated user calls `PUT /teams/{id}` with `{ "claimed": true }`; response is HTTP 200 with empty body; subsequent `GET /teams` shows that team with `claimedBy` equal to the caller's display name and `isMyTeam: true`; a player record is created if the user had none.

- **PUT /teams/{id} claim — idempotent**: given a team already claimed by the caller, `PUT /teams/{id}` with `{ "claimed": true }` returns HTTP 200 and no duplicate player record is created.

- **PUT /teams/{id} claim — conflict**: given a team claimed by a different player, `PUT /teams/{id}` with `{ "claimed": true }` returns HTTP 409.

- **PUT /teams/{id} unclaim — success**: given a team claimed by the caller, `PUT /teams/{id}` with `{ "claimed": false }` returns HTTP 200 with empty body; subsequent `GET /teams` shows `claimedBy: null` and `isMyTeam: false` for that team.

- **PUT /teams/{id} unclaim — forbidden**: given a team claimed by a different player, `PUT /teams/{id}` with `{ "claimed": false }` returns HTTP 403.

- **PUT /teams/{id} — not found**: `PUT /teams/99999` returns HTTP 404.

- **PUT /teams/{id} — schema not applied**: given schema is below V0.7, `PUT /api/v1/teams/{id}` returns HTTP 404.

- **Web — sort order**: unclaimed rows appear first, then rows owned by the signed-in user, then rows owned by other players; within each group rows are sorted alphabetically by machine name then team name.

- **Web — combined list**: the `/teams` page renders unclaimed rows with a Claim button, the caller's own rows (where `isMyTeam` is true) with an Unclaim button, and other players' rows with "Claimed by {name}" and no button.

- **Web — in-flight state**: while a Claim or Unclaim request is in flight, the button for that row is disabled.

- **Web — failure**: if the request returns an error, the row shows an inline error message specific to the failure (409 → "Already claimed by another player"; 403 → "You don't own this team"; other → "Something went wrong, please try again") and the button is re-enabled in its original state.

- **Web — empty state**: when the teams list is empty, the page shows "No teams found. Teams appear here once replays have been processed."

- **Web — load failure**: if `GET /teams` returns an error, the page shows a generic error message instead of the list.

- **Web — auth guard**: navigating to `/teams` without being signed in redirects to the landing page (existing `RequireAuth` component).

## Open Questions

None.
