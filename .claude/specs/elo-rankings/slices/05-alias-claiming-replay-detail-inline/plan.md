# Plan: Alias Claiming — Replay Detail Inline

## Context

This slice adds inline Claim buttons to the placement pills on the existing game detail page.
It builds entirely on slice 04 (alias claiming — standalone page), which delivered:
- `GET /api/v1/teams` — returns all `(machine, teamName)` pairs with `id`, `claimedBy`, and `isMyTeam`
- `PUT /api/v1/teams` — takes `{ id, claimed }` in the body (no path parameter), performs claim/unclaim
- `TeamsPage.tsx` — the standalone page that already uses those endpoints

This slice is frontend-only. No new backend endpoints, migrations, or C# changes are required.

The placement pills already exist in `GameDetailPage.tsx` inside the "Finishing order" section of the
hero card. Each pill renders a `(machine, teamName)` pair with its finish position. This slice
augments those pills to include a Claim button when the team is unclaimed, determined by
fetching `GET /api/v1/teams` and filtering client-side to the `(machine, teamName)` pairs present
in the replay's placements (case-sensitive match on both fields).

---

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx` | Add teams fetch, per-pill Claim button, and claim handler |

---

## Implementation Details

### 1. API shape confirmed from slice 04

From `src/Worms.Hub.Gateway/API/Controllers/TeamsController.cs` and
`src/Worms.Hub.Gateway/API/DTOs/TeamDtos.cs` (read directly):

- `GET /api/v1/teams` — returns `TeamDto[]` with `{ id, machine, teamName, claimedBy, isMyTeam }`
- `PUT /api/v1/teams` — `[HttpPut]` with no route suffix — body is `{ id: number, claimed: boolean }`

The `ClaimTeamDto` record is `ClaimTeamDto(int Id, bool Claimed)` — both `id` and `claimed` go
in the body. This differs from the slice 04 plan which mentioned `/teams/{id}` — the actual
implementation uses `/teams` with `id` in the body. The `TeamsPage.tsx` already uses this
correctly: `body: JSON.stringify({ id, claimed })`.

### 2. State additions in GameDetailPage

Add the following state variables to `GameDetailPage` (alongside the existing `replay`, `league`,
`error`, `notFound`, `activePanel` state):

```typescript
const [teams, setTeams] = useState<TeamDto[] | null>(null)
const [teamsRefetchKey, setTeamsRefetchKey] = useState(0)
const [pendingClaim, setPendingClaim] = useState<Set<number>>(new Set())
```

- `teams`: `null` means teams have not yet loaded or failed to load. An empty array means loaded
  but no teams. The pill rendering treats `null` the same as "still loading" — no Claim buttons
  shown in either case.
- `teamsRefetchKey`: incrementing this triggers a re-fetch after a successful claim.
- `pendingClaim`: set of team ids currently being claimed (Claim button disabled for that pill).

### 3. TeamDto interface

Add the following interface to `GameDetailPage.tsx` (alongside the existing `PlacementDto`,
`ReplayDetailDto`, etc.):

```typescript
interface TeamDto {
    id: number
    machine: string
    teamName: string
    claimedBy: string | null
    isMyTeam: boolean
}
```

### 4. Teams fetch — useEffect

Add a second `useEffect` below the existing one that fetches the replay and league. Follow the
exact pattern from `TeamsPage.tsx` (and consistent with `GameDetailPage`'s existing effect —
`.then()` chains, not `async/await` in the effect body):

```typescript
useEffect(() => {
    if (!auth.user?.access_token) return
    const token = auth.user.access_token
    fetch(`${gatewayUrl}/api/v1/teams`, {
        headers: { Authorization: `Bearer ${token}` },
    })
        .then((res) => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`)
            return res.json() as Promise<TeamDto[]>
        })
        .then((data) => setTeams(data))
        .catch(() => {
            // Silently omit Claim buttons on failure — leave teams as null
        })
}, [auth.user?.access_token, teamsRefetchKey])
```

Note: on failure, `teams` remains `null` and no Claim buttons are shown. No error message is
displayed. This matches the spec: "If GET /teams fails for any reason, the Claim Buttons are
silently omitted".

### 5. handleClaim function

Add the following function inside `GameDetailPage`. It mirrors `TeamsPage.handleClaim` but
deliberately omits error state — on failure the button silently re-enables with no message:

```typescript
async function handleClaim(id: number) {
    setPendingClaim((prev) => new Set(prev).add(id))
    try {
        const res = await fetch(`${gatewayUrl}/api/v1/teams`, {
            method: 'PUT',
            headers: {
                Authorization: `Bearer ${auth.user!.access_token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ id, claimed: true }),
        })
        if (res.ok) {
            setTeamsRefetchKey((k) => k + 1)
        }
        // On non-OK response, button silently re-enables (no error shown)
    } catch {
        // Network error — silently re-enable
    } finally {
        setPendingClaim((prev) => {
            const s = new Set(prev)
            s.delete(id)
            return s
        })
    }
}
```

Only `claimed: true` is sent — this page provides no Unclaim action (out of scope per spec).

### 6. Client-side team lookup helper

Add a helper derived value (not a separate hook — just a `const` inside the component body)
that builds a lookup map from `(machine, teamName)` to `TeamDto` for the unclaimed teams that
appear in the current replay's placements:

```typescript
const unclaimedTeamsByKey = new Map<string, TeamDto>()
if (teams !== null && replay?.placements) {
    for (const team of teams) {
        if (team.claimedBy === null) {
            const key = `${team.machine}\0${team.teamName}`
            unclaimedTeamsByKey.set(key, team)
        }
    }
}
```

The null-byte separator (`\0`) makes the composite key unambiguous (machine and teamName cannot
contain null bytes). Matching is case-sensitive on both fields — using the values as-is with no
`.toLowerCase()` or `.trim()`, consistent with the spec requirement.

This map is used when rendering each placement pill: look up
`unclaimedTeamsByKey.get(`${p.machine}\0${p.teamName}`)` to decide whether to show a Claim
button and what `id` to pass.

### 7. Placement pill augmentation

In the JSX for each placement pill (the `replay.placements.map((p, i) => ...)` block,
approximately line 543 in the current file), the pill currently renders a position badge and
team name. Augment it to also show a Claim button when the team is unclaimed.

The existing pill structure (condensed):

```tsx
<Paper key={`${p.machine}-${p.teamName}`} variant="outlined" sx={{ ... }}>
    <Box sx={{ /* position badge */ }}>{p.position ?? '?'}</Box>
    <Typography sx={{ fontWeight: 700, fontSize: 13 }}>{p.teamName}</Typography>
</Paper>
```

Change to include the Claim button after the `Typography` for team name:

```tsx
<Paper key={`${p.machine}-${p.teamName}`} variant="outlined" sx={{ ... }}>
    <Box sx={{ /* position badge — unchanged */ }}>{p.position ?? '?'}</Box>
    <Typography sx={{ fontWeight: 700, fontSize: 13 }}>{p.teamName}</Typography>
    {(() => {
        const teamKey = `${p.machine}\0${p.teamName}`
        const unclaimedTeam = unclaimedTeamsByKey.get(teamKey)
        if (!unclaimedTeam) return null
        return (
            <Button
                size="small"
                variant="outlined"
                disabled={pendingClaim.has(unclaimedTeam.id)}
                onClick={() => void handleClaim(unclaimedTeam.id)}
                sx={{ ml: 0.5, height: 22, fontSize: 11, px: 1, py: 0, minWidth: 0 }}
            >
                Claim
            </Button>
        )
    })()}
</Paper>
```

Using an IIFE (`(() => { ... })()`) inside the map keeps the lookup logic inline without
needing a dedicated sub-component or an outer `.map` restructure.

The `sx` values for the Button keep it compact enough to sit inside the pill without overflowing:
`height: 22` matches the pill's `py: 0.75` layout; `fontSize: 11` and `px: 1` match the small
chip style used elsewhere on the page. These are inline style choices — adjust if the visual
result looks off, but do not alter the surrounding pill `sx` props.

### 8. Button import

`Button` is not currently imported in `GameDetailPage.tsx`. Add it to the MUI import block:

```typescript
import Button from '@mui/material/Button'
```

This follows the existing pattern of individual named imports from `@mui/material/*` (one
component per import line).

### 9. Scope decision — list endpoint asymmetry

`GET /api/v1/teams` is a global list endpoint introduced in slice 04. This slice uses it without
modification. The list endpoint returns all teams regardless of which replay they came from;
client-side filtering by the replay's placements is the specified approach. No changes to the
list endpoint are needed — confirmed in scope for client-side filtering per spec.

### 10. Conditions under which Claim buttons appear

The Claim button is shown for placement `p` if and only if:
- `replay.placements` is non-null and non-empty (guarded by existing JSX condition)
- `teams` is non-null (loaded successfully)
- A `TeamDto` in `teams` has `machine === p.machine` AND `teamName === p.teamName` (case-sensitive)
- That `TeamDto` has `claimedBy === null`

If any condition is not met, no button is shown for that pill. This naturally handles:
- Teams loading (`teams === null`) → no buttons
- Teams load failure (`teams` stays `null`) → no buttons
- Claimed teams → `claimedBy !== null` → not in `unclaimedTeamsByKey` → no button
- Replay with no placements → outer JSX condition guards the whole block

### 11. Caveats from prior learnings

- **`useCallback` + optional-chain dependency**: do not use `useCallback` for the teams fetch.
  Put fetch logic directly in `useEffect` with `.then()` chain. Use `teamsRefetchKey` counter
  as the re-fetch trigger (from slice 04 learnings, directly applicable here).
- **Prettier formatting**: run `npx prettier --write src` inside `src/Worms.Hub.Web/` before
  committing.
- **`tsc -b` not `tsc --noEmit`**: use `tsc -b` to type-check (from web component doc).
- **`make web.lint` must pass**: run this before committing (ESLint + tsc -b + Prettier check).
- **MUI v9 scalar props in `sx`**: all style properties go in `sx`, not as direct JSX props
  (e.g. `fontWeight`, `fontSize`).

### 12. No automated tests required

The spec has no explicit test requirement for this slice. The only existing web test covers
`RequireAuth` because it is a security invariant (per web component doc: "any component that is
the single enforcement point for a security or routing invariant must have automated tests").
The placement pill Claim button is not a security invariant — it is a convenience UI feature.
`TeamsPage.tsx` also ships with no dedicated test file. No new test file is required.

---

## Verification

1. **Web lint passes**: inside `src/Worms.Hub.Web/`, run `npx prettier --write src` then
   `make web.lint` — exits 0 with no TypeScript errors.
2. **Web build passes**: `make web.build` exits 0.
3. **Teams loading — no buttons**: navigate to a game detail page; before the GET /teams response
   arrives, the placement pills show no Claim buttons.
4. **Unclaimed team — Claim button present**: with a processed replay where at least one team
   is unclaimed in the database, the pill for that team shows a Claim button.
5. **Claimed team — no button**: a pill for a team with `claimedBy` set shows no Claim button
   and no other indicator.
6. **Claim in flight — button disabled**: click Claim on a pill; while the PUT is in flight
   that pill's button is disabled; other pills are unaffected.
7. **Claim success**: after a successful PUT, GET /teams is re-fetched; the Claim button
   disappears from the claimed pill.
8. **Claim failure — silent re-enable**: when the PUT returns a non-200 or network error, the
   Claim button re-enables with no error message shown.
9. **Teams load failure — no buttons**: with GET /teams returning an error, the placements area
   renders normally (position badges and team names visible) but no Claim buttons appear.
10. **No placements — no buttons**: a replay with `placements: null` or `placements: []` renders
    no Claim buttons (the outer JSX condition already guards this block).
