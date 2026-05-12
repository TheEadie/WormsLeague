# League List

## Overview

A signed-in member can navigate to `/leagues` and see a list of all leagues served by the Hub. This slice introduces a `leagues` database table as the source of truth for leagues, adds a Gateway list endpoint backed by that table, builds the SPA page, and redirects the post-sign-in flow to land users there.

## Requirements

- A `leagues` table is added to the database via a Flyway migration. It stores at minimum a league's id and name.
- "redgate" is added to the local-dev seed data so the league list is populated in a `docker compose` environment.
- The Gateway exposes a new `GET /api/v1/leagues` endpoint that returns all leagues as an array of `LeagueDto` (Id, Name, Version, SchemeUrl). For each row in the `leagues` table the controller fetches the corresponding scheme details from the filesystem. The endpoint is protected by the existing `[Authorize]` contract.
- The existing `GET /api/v1/leagues/{id}` endpoint is updated to first verify the requested id exists in the `leagues` table, then fetch the scheme details from the filesystem as it does today. It returns 404 if the id is not found in the database.
- The SPA has a `/leagues` route protected by `RequireAuth`.
- The page fetches `GET /api/v1/leagues` with the signed-in user's bearer token and displays the results as a list of league cards.
- Each card shows the league's name, id, and scheme version.
- Each card is a link to `/leagues/{id}` (the per-league page, built in the next slice).
- While the fetch is in progress, a loading indicator is shown.
- If the fetch fails (network error or non-2xx response), an error message is shown.
- After completing the OIDC sign-in callback, the user is redirected to `/leagues` instead of `/authenticated`.
- The placeholder `/authenticated` route and its page component are removed.

## Out of Scope

- Game count, player avatars, last match, season leader, or any other data per league card beyond what `LeagueDto` provides (Id, Name, Version, SchemeUrl).
- League descriptions, season/week labels, or colour coding.
- The per-league page content — `/leagues/{id}` is a dead link in this slice.
- Any search, filter, or sort on the league list.
- Preserving the originally-requested URL before sign-in and redirecting back after.
- Populating the `leagues` table in production (that is a deployment/ops concern outside this slice).
- Changing the behaviour of `GET /api/v1/leagues/{id}` beyond what is described in the requirements below.

## Acceptance Criteria

- A signed-in user who navigates to `/leagues` sees a card for each league in the `leagues` table. Each card displays the league's name, id, and scheme version.
- Each league card is a link that navigates to `/leagues/{id}`.
- A signed-out user who navigates to `/leagues` is redirected to `/` (the landing page) by the existing `RequireAuth` wrapper.
- After completing the OIDC sign-in flow (i.e. after the `/callback` redirect), the user lands on `/leagues`.
- While the API call is in progress, a loading indicator is visible.
- If the API call fails, an error message is displayed.
- `GET /api/v1/leagues` returns HTTP 200 with an array of `LeagueDto` objects when the table has rows, and HTTP 200 with an empty array when it has none.
- `GET /api/v1/leagues` returns HTTP 401 for unauthenticated requests.
- `GET /api/v1/leagues/{id}` returns HTTP 404 when the id is not present in the `leagues` table.
- `GET /api/v1/leagues/{id}` returns HTTP 401 for unauthenticated requests.
- Under `docker compose up`, the "redgate" league appears in the list.
- `make web.build` and `make web.lint` both pass with the changes in place.

## Open Questions

None.
