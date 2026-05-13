# Game Detail Page

## Overview

A signed-in member can navigate to `/leagues/{id}/replays/{replayId}` and see a detailed view of a single game — its participants, winner, date, scheme version, and a turn-by-turn breakdown with weapon usage stats — all driven by parsing the stored replay log on demand.

## Requirements

### Gateway

- A new authenticated `GET /api/v1/leagues/{id}/replays/{replayId}` endpoint returns a single replay's details.
- The response includes hero data (id, name, status, date, winner, and the list of participating team names) and, when the replay has a stored log, a parsed turns array.
- Each turn in the turns array includes: turn number, the playing team's name, the ordered list of weapons fired that turn (name only), and the damage dealt that turn (one entry per target team: team name, health lost, worms killed).
- The endpoint parses the stored `full_log` on demand using the existing `IReplayTextReader` pipeline; no new database tables are added.
- Returns 404 when the replay ID does not exist, or when the replay belongs to a different league than the `{id}` in the path.
- Returns hero data with an absent turns array when the replay has no stored log.
- Returns 401 for unauthenticated requests, consistent with the existing `[Authorize]` contract on all Gateway endpoints.

### SPA

- A new `/leagues/:id/replays/:replayId` route is added to `App.tsx`, protected by the existing `RequireAuth` wrapper.
- On load, the page fetches the single-replay endpoint and `GET /api/v1/leagues/{id}` concurrently using the signed-in user's bearer token.
- While either fetch is in progress, a loading indicator is shown.
- If either fetch returns a non-2xx response, an error message is shown.
- If the replay endpoint returns 404, a not-found message is shown.
- If the replay's status is not `Processed`, a "processing" message is shown in place of all other content.
- For processed replays, a hero card is shown containing:
  - Title: "Match #00x" where x is the replay's database ID zero-padded to three digits.
  - Date and time of the game.
  - A winner chip showing the winning team name, or "Draw" with neutral styling for drawn games. Omitted if `winner` is null.
  - Chips for all participating team names.
  - A scheme version chip (e.g. "Scheme v1.2"), omitted entirely if the league has no scheme version.
  - A stats strip with four figures computed from the turns array: Duration, Turns, Max Damage, and Kills. The entire stats strip is omitted if there are no turns.
    - **Duration**: elapsed time from the first turn's start timestamp to the last turn's end timestamp; if the last turn has no end timestamp, use the latest available timestamp from that turn's weapon or damage events, or the turn's start timestamp as a last resort.
    - **Turns**: total number of turns.
    - **Max Damage**: sum of `healthLost` across all damage records (including self-damage) for the single turn with the highest total damage.
    - **Kills**: total `wormsKilled` summed across all damage records in all turns, including self-kills.
- A breadcrumb shows: Leagues (links to `/leagues`) → {League Name} (links to `/leagues/{id}`) → Match #00x (current page, no link). The league name is sourced from the concurrent `GET /api/v1/leagues/{id}` response.
- The page uses the sidebar layout from the design mockup: a left-hand nav panel listing the available panels, and the selected panel's content area on the right.

### Turn-by-turn panel

- Shown by default when the page loads.
- Displays one row per turn, in turn order.
- Each row shows: turn number, team name, the ordered list of weapons fired that turn, and the damage dealt that turn.
  - The last weapon in the list is visually distinguished as the damaging weapon (e.g. bold or labelled).
  - Turns with no weapons fired show "—" in the weapons column.
  - Damage is shown as one entry per target team, with health lost and kill count; kills are shown only when non-zero.
  - Turns with no damage show "—" in the damage column.
- When the turns array is absent or empty, an appropriate empty-state message is shown in place of the table.

### Weapons panel

- Displays weapon usage grouped by team.
- For each team, lists each distinct weapon that team used across all their turns.
- Each weapon entry shows: weapon name, usage count (number of turns in which that weapon appears in the weapons list for that team), and attributed damage (sum of total damage dealt in turns where that weapon was the last weapon fired by that team).
- Entries for each team are sorted by attributed damage descending, with usage count as a tiebreaker.
- When the turns array is absent or empty, an appropriate empty-state message is shown.

### Local-dev seed data

- At least one seeded replay in `R__ReplaysTestData.sql` has its `full_log` column set to a realistic WA log string, including team declarations, at least four turns with weapons and damage records, and a winner line — giving the Turn-by-turn and Weapons panels real data to display under `docker compose`.

## Out of Scope

- Team colour coding on chips — deferred pending a review of how team colours are stored in the database.
- Replay file download link — covered by the next slice (Replay viewer).
- Damage chart panel — deferred pending a way to track remaining worm health at the point of kill.
- Head-to-head panel — removed from scope.
- Global error boundary — a separate future slice.
- Pagination of the turns list.
- Auto-refresh for unprocessed replays — the user must manually refresh the page.

## Acceptance Criteria

- A signed-in user navigating to `/leagues/redgate/replays/{id}` for a processed replay with turn data sees the "Match #00x" hero card, the correct date and time, the winner chip, participating team chips, the scheme version chip, and the stats strip.
- Duration, Turns, Max Damage, and Kills in the stats strip reflect correct values computed from the replay's turn data.
- A drawn game shows "Draw" in the winner chip with neutral (non-team) styling.
- The scheme version chip is absent when the league has no scheme version.
- The stats strip is absent when the replay has no turn data.
- The breadcrumb shows Leagues → {League Name} → Match #00x; the first two items are working links, the last is plain text.
- The Turn-by-turn panel is active and visible on first load; clicking "Weapons" in the sidebar switches the content area to the Weapons panel.
- The Turn-by-turn panel shows one row per turn in order; every turn is shown, including turns with no weapons and turns with no damage.
- The last weapon in a turn's weapon list is visually distinguished from earlier weapons in that turn.
- Turns with no weapons show "—" in the weapons column; turns with no damage show "—" in the damage column.
- The Weapons panel shows per-team weapon entries with attributed damage, sorted by attributed damage descending.
- For a processed replay with no stored log, the hero card is shown but both Turn-by-turn and Weapons panels display an empty-state message.
- Navigating directly to a replay with status `Pending` shows a "processing" message with no hero card or panels.
- Navigating to a non-existent replay URL (or one belonging to a different league) shows a not-found message.
- A non-2xx response from either concurrent API call shows an error message.
- A signed-out user navigating to `/leagues/{id}/replays/{replayId}` is redirected to `/` by the existing `RequireAuth` wrapper.
- `GET /api/v1/leagues/{id}/replays/{replayId}` returns 401 for unauthenticated requests.
- `GET /api/v1/leagues/{id}/replays/{replayId}` returns 404 when the replay does not exist or its league does not match the path.
- Under `docker compose up`, at least one seeded replay's detail page shows a hero card with a populated stats strip and populated Turn-by-turn and Weapons panels.
- `make web.build` and `make web.lint` both pass with the changes in place.

## Open Questions

None.
