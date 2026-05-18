# Plan: ELO Leaderboard on League Cards

## Context

This slice is the final piece of the ELO Rankings epic. Slice 06 (ELO Rankings) introduced the `standings: StandingDto[] | null` field on `GET /api/v1/leagues` and `GET /api/v1/leagues/{id}` and rendered a standings table on the league detail page. This slice surfaces a top-3 leaderboard preview on each league card on the `/leagues` list page, consuming the existing `standings` field with no backend changes. Verified from `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` (`GetAll`): when the ELO ratings feature flag is enabled the controller already populates `standings` for every league in the list response, so the Web UI only needs to read and render it.

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx` | Add `StandingDto` interface; extend `LeagueDto` with `standings: StandingDto[] | null`; render a leaderboard section inside `LeagueCard` showing the top 3 standings entries below the existing scheme chip, separated by a divider, with caption `LEADERBOARD` and `top N of M` footer. |

---

## Implementation Details

### 1. Type additions in `LeagueListPage.tsx`

Add a `StandingDto` interface above the existing `LeagueDto` interface that exactly matches the shape used on `LeagueDetailPage.tsx` (verified at lines 23–27 of that file):

```ts
interface StandingDto {
    playerName: string
    elo: number
    gamesPlayed: number
}
```

Extend `LeagueDto` (currently at lines 15–20) by adding `standings: StandingDto[] | null` as the final property. Do not change the other existing fields.

The `fetch(...)` call at line 89 already casts the response to `LeagueDto[]`; no change is needed there — the field will be picked up automatically once the interface is widened.

### 2. New leaderboard section inside `LeagueCard`

Render the leaderboard inside the existing `<CardContent>` block, after the existing `Chip` for `Scheme v…` (currently the last child, at lines 68–75). Wrap the leaderboard in a guard so it is omitted entirely when `standings` is `null` or an empty array:

```tsx
{league.standings !== null && league.standings.length > 0 && (
    <>
        <Divider sx={{ mt: 2, mb: 1.5 }} />
        <Box>
            <Box
                sx={{
                    display: 'flex',
                    alignItems: 'baseline',
                    justifyContent: 'space-between',
                    mb: 1,
                }}
            >
                <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ textTransform: 'uppercase', letterSpacing: '0.1em' }}
                >
                    Leaderboard
                </Typography>
                <Typography
                    sx={{
                        fontFamily: monoFontFamily,
                        fontSize: 10,
                        color: 'text.disabled',
                    }}
                >
                    top {Math.min(3, league.standings.length)} of {league.standings.length}
                </Typography>
            </Box>
            <Stack spacing={0.5}>
                {league.standings.slice(0, 3).map((s, index) => {
                    const place = index + 1
                    const medal = (['#ffca28', '#bdbdbd', '#cd7f32'] as const)[index]
                    return (
                        <Box
                            key={index}
                            sx={{
                                display: 'grid',
                                gridTemplateColumns: '22px 1fr auto',
                                gap: 1,
                                alignItems: 'center',
                            }}
                        >
                            <Typography
                                sx={{
                                    fontFamily: monoFontFamily,
                                    fontSize: 12,
                                    fontWeight: 700,
                                    color: medal,
                                    textAlign: 'center',
                                }}
                            >
                                {place}
                            </Typography>
                            <Typography
                                variant="body2"
                                sx={{
                                    fontWeight: place === 1 ? 700 : 500,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis',
                                    whiteSpace: 'nowrap',
                                }}
                            >
                                {s.playerName}
                            </Typography>
                            <Typography
                                sx={{
                                    fontFamily: monoFontFamily,
                                    fontSize: 13,
                                    fontWeight: 700,
                                    color: 'primary.main',
                                    textAlign: 'right',
                                }}
                            >
                                {s.elo}
                            </Typography>
                        </Box>
                    )
                })}
            </Stack>
        </Box>
    </>
)}
```

Notes:

- The caption text uses `Leaderboard` with `textTransform: 'uppercase'` applied via `sx` (consistent with the design reference). The spec requires the rendered text to read "LEADERBOARD"; using `textTransform: 'uppercase'` satisfies this and matches the pattern used on the design mock.
- The footer string is computed as `top ${Math.min(3, M)} of ${M}`, so for `M=1` it renders `top 1 of 1`, for `M=2` it renders `top 2 of 2`, etc.
- Rows iterate `standings.slice(0, 3)` so when fewer than 3 entries exist only the present rows render.
- The whole card remains a single `<Link to={...}>` (lines 24–27); no per-row `onClick` or per-row `<Link>` is added, satisfying the "leaderboard rows are not individually interactive" requirement.
- Player-name truncation uses inline `overflow: hidden` + `textOverflow: ellipsis` + `whiteSpace: nowrap` on the `Typography`'s `sx`. Do not use `noWrap` as a prop because the grid column `1fr` already provides the width constraint; the explicit `sx` form keeps the rule local and avoids interaction issues seen with MUI v9 prop shorthands (see `.claude/docs/components/web.md`).

### 3. Required new imports

Add the following imports to `LeagueListPage.tsx` (the file currently imports `Box`, `Breadcrumbs`, `Card`, `CardContent`, `Chip`, `CircularProgress`, `Container`, `Typography`):

```ts
import Divider from '@mui/material/Divider'
import Stack from '@mui/material/Stack'
```

`monoFontFamily` is already imported (line 12); no change.

### 4. Formatting and linting

After the edit:

1. Run `npm ci` once inside `src/Worms.Hub.Web/` if `node_modules/` is not already present (see slice 05 learnings — `make web.lint` fails with `ERR_MODULE_NOT_FOUND` otherwise).
2. Run `npx prettier --write src/pages/LeagueListPage.tsx` from `src/Worms.Hub.Web/` to align indentation, trailing commas, and quote style with the rest of the file. Slice 03 learnings explicitly call this out as a recurring issue.
3. Run `make web.lint` from the repo root and ensure ESLint, `tsc -b`, and Prettier all pass. Pay particular attention to the React Compiler ESLint rule (`react-compiler/react-compiler`) — the leaderboard JSX is pure rendering with no impure calls (no `Math.random`, no `Date.now`), so no `useState` initialiser tricks are needed.
4. Run `make web.build` from the repo root and ensure the bundle builds cleanly.

### 5. Scope decision — list vs detail symmetry

The list endpoint (`GET /api/v1/leagues`) and the single-item endpoint (`GET /api/v1/leagues/{id}`) already return identical `standings` shapes, populated by the same `RatingsRepository.GetByLeagueId(...)` call (verified in `LeaguesController.cs`). No API-side symmetry work is required. The Web UI detail page (`LeagueDetailPage.tsx`) already renders the full standings table from this data and is **explicitly out of scope** for this slice — it must not be modified.

### 6. Out-of-scope guard rails

- No changes to `LeagueDetailPage.tsx`, the Gateway, the storage layer, the database, or any tests.
- No new test infrastructure (the web component currently has no Vitest tests for these pages; this slice does not introduce them — consistent with the existing pattern for purely visual list/card components).
- No empty-state placeholder when `standings` is `null` or empty — the entire leaderboard subtree must be omitted (the guard above ensures this).

---

## Verification

1. `make web.lint` passes (ESLint, `tsc -b`, Prettier).
2. `make web.build` passes (Vite production build).
3. Manually load `/leagues` with the local dev stack against a database where at least one league has 3+ ELO-rated players: the card renders a `LEADERBOARD` section with 3 rows ranked gold/silver/bronze and a footer `top 3 of M`.
4. For a league with `standings.length === 2`, the same card renders exactly 2 rows and footer reads `top 2 of 2`. For `standings.length === 1`, 1 row and `top 1 of 1`. For an empty array or `null`, no leaderboard section, divider, caption, or footer is rendered on that card.
5. Clicking anywhere on the card (including over the leaderboard area) navigates to `/leagues/{id}` — confirmed because the entire `Card` is still wrapped in the existing `<Link to={…}>` at the top of `LeagueCard`.
6. Resizing the window or rendering a deliberately long player name confirms the name truncates with an ellipsis on a single line; ELO values remain right-aligned and unmoved.
7. The standings table on `/leagues/{id}` is unchanged (visual comparison before/after).
