# Per-League Page

## Overview

A signed-in member can navigate to `/leagues/{id}` and see the scheme used by that league and a list of its historic replays. This slice adds `league_id`, `date`, `winner`, and `teams` columns to the `replays` table, backfills them from existing log data, extends the Gateway with a per-league replays endpoint, and builds the SPA page that consumes it.

## Requirements

### Database

- The `replays` table gains four new nullable columns: `league_id` (text, FK to `leagues.id`), `date` (timestamp), `winner` (text), and `teams` (list of team name strings).
- A Flyway migration adds these columns.
- A Flyway SQL migration sets `league_id = 'redgate'` for all existing rows in the `replays` table (redgate is the only league that exists in production), and populates `date`, `winner`, and `teams` for existing rows that already have `full_log` set, using PostgreSQL regex extraction directly against the known log format patterns:
  - `date`: extracted from the line matching `Game Started at YYYY-MM-DD HH:MM:SS GMT`
  - `winner`: extracted from the line matching `<name> wins the match!` / `<name> wins the round.`, or set to `"Draw"` when the line `The round was drawn.` is present
  - `teams`: extracted from lines matching either the online pattern (`Colour: "player" as "team"`) or the offline pattern (`Colour: "team"`), collecting the team name from each matching line into an array
- The Worker Processor is updated to also store `date`, `winner`, and `teams` when it successfully processes a new replay.
- The local-dev seed file `R__ReplaysTestData.sql` is updated so that at least one replay is associated with the `redgate` league and has `date`, `winner`, and `teams` populated, giving the per-league page something to display under `docker compose`.

### Gateway

- A new `GET /api/v1/leagues/{id}/replays` endpoint returns the replays belonging to the given league, ordered by `date` descending (newest first).
- Each item in the response includes: `id`, `name`, `processed` (bool), `date` (nullable), `winner` (nullable), and `teams` (nullable list of team name strings). `processed` is `true` when the replay has been fully processed (i.e. `date`, `winner`, and `teams` are populated); `false` when processing is still pending.
- The endpoint returns `404` when the given league `id` is not present in the `leagues` table.
- The endpoint returns `200` with an empty array when the league exists but has no replays.
- The endpoint is protected by the existing `[Authorize]` contract and returns `401` for unauthenticated requests.

### SPA

- A new `/leagues/{id}` route is added, protected by the existing `RequireAuth` wrapper.
- The page fetches `GET /api/v1/leagues/{id}` and `GET /api/v1/leagues/{id}/replays` concurrently, using the signed-in user's bearer token.
- The page displays the league's name as its heading.
- The page displays the scheme's version number and a download link to the scheme file (from `SchemeUrl` in the league response).
- The page displays a list of replays. For processed replays (`processed: true`) each row shows date, winner team name, and the participating team names. For unprocessed replays (`processed: false`) the row shows the replay name and a holding message indicating the data will appear once processing is complete.
- Each replay row is a link to `/leagues/{id}/replays/{replayId}` (the game detail page, built in the next slice — a dead link in this slice).
- While either fetch is in progress, a loading indicator is shown.
- If either fetch returns a non-2xx response, an error message is shown.
- If the leagues fetch returns `404`, an appropriate "not found" message is shown instead of the league content.
- If the league has no replays, an appropriate empty-state message is shown in place of the list.
- The league cards on the `/leagues` list page already link to `/leagues/{id}`; no change to those links is needed.

## Out of Scope

- Search, filter, or sort controls on the replay list — v1 shows all replays for the league ordered by date, newest first.
- Pagination of the replay list.
- Any per-replay statistics in the list beyond date, winner, and participating team names (e.g. damage totals, top weapons, map name, turn count) — those belong to the game detail page or later slices.
- Storing full team details (machine, colour) — only team names are stored and returned in this slice.
- Any changes to the scheme content or format displayed beyond version number and download link.
- Upload replay or leaderboard actions shown in the design mockup — not in scope for this epic.

## Acceptance Criteria


- A signed-in user who navigates to `/leagues/redgate` sees the league name "Redgate", the scheme version number, and a working scheme download link.
- The replay list shows one row per replay associated with the `redgate` league. Processed replay rows display the date, winner team name, and participating team names. Unprocessed replay rows display the replay name and a holding message.
- Each replay row is a link to `/leagues/redgate/replays/{replayId}`.
- A signed-out user who navigates to `/leagues/{id}` is redirected to `/` by the existing `RequireAuth` wrapper.
- Navigating to `/leagues/nonexistent-league` shows a "not found" message rather than a blank or broken page.
- While the API calls are in progress, a loading indicator is visible.
- If either API call fails (network error or non-2xx response), an error message is displayed.
- Under `docker compose up`, the "redgate" league page shows at least one replay with date, winner, and teams populated.
- `GET /api/v1/leagues/redgate/replays` returns HTTP 200 with a non-empty array in the `docker compose` environment.
- `GET /api/v1/leagues/{id}/replays` returns HTTP 200 with an empty array for a league that exists but has no replays.
- `GET /api/v1/leagues/{id}/replays` returns HTTP 404 when the league id is not in the `leagues` table.
- `GET /api/v1/leagues/{id}/replays` returns HTTP 401 for unauthenticated requests.
- `make web.build` and `make web.lint` both pass with the changes in place.

## Open Questions

None.
