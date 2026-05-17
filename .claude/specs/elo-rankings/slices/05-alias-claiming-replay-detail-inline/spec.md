# Alias Claiming — Replay Detail Inline

## Overview

Extend the existing game detail page to show a Claim button on the placement pill for each unclaimed `(machine, team name)` pair in that replay, reusing the existing `GET /teams` and `PUT /teams` API endpoints from the standalone alias claiming page.

## Requirements

- The existing placement pills on the game detail page are augmented to include a **Claim** button when the corresponding team is unclaimed (i.e. `claimedBy` is `null` in the teams list).
- Teams that are already claimed — whether by the signed-in user or by another player — show no additional indicator or action on their pill.
- Team data is fetched from the existing `GET /api/v1/teams` endpoint and filtered client-side to the `(machine, teamName)` pairs present in the replay's placements. Matching is case-sensitive on both fields.
- While `GET /api/v1/teams` is loading, pills render without Claim buttons; buttons appear once the data arrives.
- The Claim button on a pill calls `PUT /api/v1/teams` with `{ id, claimed: true }`.
- While a claim request is in flight, the Claim button for that pill is disabled. Other pills are unaffected.
- If a claim request fails for any reason, the Claim button silently re-enables (no error message is shown).
- After a successful claim, `GET /api/v1/teams` is re-fetched; the pills update based on the refreshed data.
- If `GET /teams` fails to load for any reason, the Claim buttons are silently omitted — the placements area renders as normal without any claim actions.
- If the replay has no placements (`placements` is `null` or empty), no Claim buttons are shown.
- No Unclaim action is provided on this page.
- This is a frontend-only change — no new backend endpoints or migrations are required.

## Out of Scope

- Unclaim action on the game detail page.
- Any indication on the pill that a team has been claimed (by the user or anyone else) beyond the absence of a Claim button.
- Error messages on failed claim requests.
- A new backend endpoint scoped to a replay's teams — the global `GET /teams` endpoint with client-side filtering is sufficient.
- Showing which player claimed a team.
- Any ELO or rating-related content.

## Acceptance Criteria

- **Unclaimed team — Claim button present**: given a replay with at least one processed placement where the team is unclaimed, the placement pill for that team displays a Claim button.

- **Claimed team — no action**: given a placement where the team is claimed (by any player, including the signed-in user), the pill shows no Claim button and no additional indicator.

- **Claim — success**: given an unclaimed team pill with a Claim button, when the user clicks it and the PUT returns 200, `GET /teams` is re-fetched and the Claim button disappears from that pill; other pills are unaffected.

- **Claim — in flight**: while the PUT request is in flight, the Claim button for that pill is disabled; other pills and their buttons are unaffected.

- **Claim — failure**: when the PUT request fails (any non-200 response or network error), the Claim button silently re-enables with no error message.

- **Teams loading**: while `GET /teams` is in flight, no Claim buttons are shown; they appear once the response arrives.

- **Teams load failure**: when `GET /teams` returns an error, the placements area renders normally with no Claim buttons shown.

- **No placements**: when `replay.placements` is `null` or empty, no Claim buttons are rendered.

- **Multiple unclaimed teams**: when a replay contains multiple unclaimed teams, each unclaimed pill independently shows a Claim button; claiming one does not affect the others.

## Open Questions

None.
