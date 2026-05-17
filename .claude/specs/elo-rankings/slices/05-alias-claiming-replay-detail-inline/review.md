# Review — Alias Claiming: Replay Detail Inline

## Verdict

The implementation satisfies all nine acceptance criteria. The diff modifies only `GameDetailPage.tsx`, which is the sole file the plan identified. The implementation chose to extract a named `PlacementPill` component rather than use the IIFE the plan described — a positive deviation that improves readability. `make web.lint` exits clean (ESLint, `tsc -b`, Prettier all pass). There are no blockers. One suggestion: `PlacementPill` could move to its own file now that it is a named component rather than inline logic.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Unclaimed team — Claim button present | MET | `GameDetailPage.tsx:136–152` — `{unclaimedTeam && <Button>Claim</Button>}` inside `PlacementPill`; `unclaimedTeam` is passed from `unclaimedTeamsByKey` which only contains teams with `claimedBy === null` |
| Claimed team — no action | MET | `GameDetailPage.tsx:531` — only teams with `claimedBy === null` enter the map; claimed teams return `undefined` from the lookup, so the Button branch is not rendered |
| Claim — success | MET | `GameDetailPage.tsx:513–514` — `if (res.ok) { setTeamsRefetchKey((k) => k + 1) }` triggers re-fetch via the `teamsRefetchKey` dependency at line 500 |
| Claim — in flight | MET | `GameDetailPage.tsx:140` — `disabled={pendingClaim.has(unclaimedTeam.id)}`; `pendingClaim` is keyed per team id so other pills are unaffected |
| Claim — failure | MET | `GameDetailPage.tsx:517–519,520–525` — `catch {}` swallows all errors; `finally` unconditionally removes the id from `pendingClaim`, re-enabling the button with no message |
| Teams loading | MET | `GameDetailPage.tsx:457` — `teams` initialises as `null`; `GameDetailPage.tsx:529` — map only populated when `teams !== null`; no buttons rendered until fetch resolves |
| Teams load failure | MET | `GameDetailPage.tsx:497–499` — `catch` leaves `teams` as `null`, so map stays empty and no Claim buttons appear |
| No placements | MET | `GameDetailPage.tsx:674` — outer JSX condition `replay.placements !== null && replay.placements.length > 0` gates the entire pill block |
| Multiple unclaimed teams | MET | Each pill resolves its own key from `unclaimedTeamsByKey` independently; `pendingClaim` tracks ids individually |

## Scope

The plan lists one modified file: `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx`. The diff contains exactly that one file. The `.claude/specs/elo-rankings/plan.md` change in the branch is a workflow artefact (epic-level slice-complete marker) and is ignored per review rules.

The implementation deviated from the plan on pill rendering: the plan described an IIFE (`{(() => { ... })()}`) inside the `.map`, but the implementation extracted a named `PlacementPill` function component instead. This is an improvement — it eliminates seven levels of nesting and makes the component unit-testable. `learnings.md` does not mention this deviation but it is strictly better than the plan's approach, and the acceptance criteria are all met.

## Blockers

None.

## Suggestions

#### S1 — Extract `PlacementPill` to its own file

- **File:** `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx:81–156`
- **Issue:** `PlacementPill`, its props interface, and `TeamDto` now live in `GameDetailPage.tsx` alongside 5+ other local interfaces and utility functions. As a named component it is self-contained and easy to move; leaving it here makes the file harder to navigate and prevents reuse on any future page that renders placement results.
- **Fix:** Move `PlacementPillProps`, `TeamDto`, and `PlacementPill` to `src/Worms.Hub.Web/src/pages/PlacementPill.tsx`; import from there in `GameDetailPage.tsx`.
- **Decision:** Accept

## Nitpicks

None.

## Tests

No new test files were added. The plan explicitly documents this decision (section 12): the placement pill Claim button is not a security or routing invariant, and `TeamsPage.tsx` — the template for this feature — also ships without a dedicated test file. Consistent with the web component doc rule that automated tests are required only for "the single enforcement point for a security or routing invariant".

## Recommended Actions

- **S1** — Accept — `PlacementPill` is already a self-contained named component; the move is mechanical and improves file navigability and potential reuse.
