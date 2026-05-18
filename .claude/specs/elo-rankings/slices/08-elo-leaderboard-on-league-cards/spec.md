# ELO Leaderboard on League Cards

## Overview

Extend each league card on the `/leagues` page to show a top-3 ELO leaderboard preview, sourced from the `standings` field already returned by `GET /api/v1/leagues`. No backend changes are required — this is a Web UI–only slice that surfaces existing data.

## Requirements

- The `LeagueDto` interface in `LeagueListPage.tsx` is extended to include `standings: StandingDto[] | null`, with `StandingDto` shaped as `{ playerName: string; elo: number; gamesPlayed: number }`, matching the Gateway response and the existing definition on `LeagueDetailPage.tsx`.
- Each league card on the `/leagues` page renders a leaderboard section showing the top 3 entries from `standings`.
- The leaderboard section appears inside the existing card, below the existing scheme chip, and is separated from it by a horizontal divider.
- Above the rows, an uppercase caption reads `LEADERBOARD`. To the right of (or below) the caption, a small mono-style label reads `top N of M`, where:
  - `M` is the total number of entries in `standings`.
  - `N` is `min(3, M)`, i.e. it always reflects the actual number of rows rendered.
- Each leaderboard row shows three pieces of data, in this order:
  - Rank (`1`, `2`, `3`), with the rank number styled in the medal colours used on the league detail page (`#ffca28` gold, `#bdbdbd` silver, `#cd7f32` bronze).
  - Player name.
  - ELO rating, in the mono font, right-aligned.
- If a league has fewer than 3 rated players, only the rows that exist are rendered (no placeholder rows).
- The leaderboard section is omitted entirely when `standings` is `null` or an empty array — no caption, footer, divider, or placeholder is rendered in that case.
- Player names that are too long to fit on one line are truncated with an ellipsis (single-line, no wrap).
- Leaderboard rows preserve the order returned by the API (already ELO-descending); no client-side resorting or tiebreaker logic is added.
- The entire card remains a single link to `/leagues/{id}` — clicking anywhere on the card, including the leaderboard area, navigates to the per-league page. Leaderboard rows are not individually interactive.

## Out of Scope

- Any backend or API changes — `GET /api/v1/leagues` is consumed as-is.
- Per-row data beyond rank + name + ELO (no avatars, no colour badges, no wins, no games-played on the row).
- Per-player links from leaderboard rows (no player pages exist yet).
- Showing the leaderboard anywhere other than the `/leagues` cards (the existing detail page standings table is untouched).
- An empty-state message when `standings` is `null` or empty — the section is silently omitted instead.
- Per-card loading skeletons — the existing page-level `CircularProgress` is unchanged.
- Visual changes to the league cards outside the new leaderboard section.
- Changes to the API response shape, ordering guarantees, or tiebreaker behaviour.

## Acceptance Criteria

- **Card with 3+ rated players**: when a league's `standings` array contains 3 or more entries, the card renders a leaderboard with exactly 3 rows in the order returned by the API; the caption reads `LEADERBOARD` and the footer reads `top 3 of M` where M is `standings.length`.
- **Card with 2 rated players**: when `standings.length === 2`, the leaderboard renders 2 rows and the footer reads `top 2 of 2`.
- **Card with 1 rated player**: when `standings.length === 1`, the leaderboard renders 1 row and the footer reads `top 1 of 1`.
- **Card with empty standings**: when `standings` is an empty array, no leaderboard section is rendered on the card — no caption, no divider, no footer.
- **Card with null standings**: when `standings` is `null` (e.g. before the ELO schema migration has been applied), no leaderboard section is rendered on the card.
- **Rank medal colours**: the first row's rank number is rendered in gold (`#ffca28`), the second in silver (`#bdbdbd`), the third in bronze (`#cd7f32`).
- **ELO typography**: ELO values use the existing `monoFontFamily` from `theme.ts` and are right-aligned within their row.
- **Long player names**: a player name that exceeds the available width is truncated to a single line with an ellipsis.
- **Card click target**: clicking the leaderboard area navigates to `/leagues/{id}`; no separate click handler is attached to rows.
- **Existing card content unchanged**: the league id, name, and scheme version chip continue to render exactly as before, in the same positions.
- **Detail page unchanged**: the standings table on `/leagues/{id}` is not modified by this slice.
- **No API changes**: `GET /api/v1/leagues` and `GET /api/v1/leagues/{id}` return the same payloads as before this slice was implemented.
- **Lint and build**: `make web.lint` and `make web.build` both pass with the changes in place.

## Open Questions

None.
