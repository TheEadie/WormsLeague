# Review — Alias Claiming: Replay Detail Inline

## Verdict

The implementation satisfies all nine acceptance criteria from the spec. The single modified file — `GameDetailPage.tsx` — follows the plan exactly: `Button` import, `TeamDto` interface, three state variables, a second `useEffect` for the teams fetch, a `handleClaim` async function, the `unclaimedTeamsByKey` map, and the IIFE-based pill augmentation all match the plan's code snippets. All three `make web.lint` sub-checks (ESLint, `tsc -b`, Prettier) exit clean. There are no blockers.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Unclaimed team — Claim button present | MET | `GameDetailPage.tsx:662–691` — IIFE looks up `unclaimedTeamsByKey` and renders `<Button>Claim</Button>` when team is found |
| Claimed team — no action | MET | `GameDetailPage.tsx:451–458` — only teams with `claimedBy === null` are inserted into `unclaimedTeamsByKey`; claimed teams fall through IIFE returning `null` |
| Claim — success | MET | `GameDetailPage.tsx:436–437` — `if (res.ok) { setTeamsRefetchKey((k) => k + 1) }` triggers re-fetch; `useEffect` at line 409 depends on `teamsRefetchKey` |
| Claim — in flight | MET | `GameDetailPage.tsx:426,671–673` — `setPendingClaim` adds id on entry; Button has `disabled={pendingClaim.has(unclaimedTeam.id)}`; only that pill's id is in the set |
| Claim — failure | MET | `GameDetailPage.tsx:440–447` — `catch {}` swallows all errors; `finally` removes id from `pendingClaim` unconditionally, re-enabling the button |
| Teams loading | MET | `GameDetailPage.tsx:380` — `teams` initialises as `null`; `GameDetailPage.tsx:451–452` — map is only populated when `teams !== null`; no buttons render until fetch resolves |
| Teams load failure | MET | `GameDetailPage.tsx:420–422` — `catch` block leaves `teams` as `null`, so `unclaimedTeamsByKey` remains empty and no buttons appear |
| No placements | MET | `GameDetailPage.tsx:597` — outer JSX condition `replay.placements !== null && replay.placements.length > 0` gates the entire pill block; `GameDetailPage.tsx:452` — map also checks `replay?.placements` |
| Multiple unclaimed teams | MET | Each pill independently looks up its own key in `unclaimedTeamsByKey`; `pendingClaim` tracks ids individually so claiming one does not affect others |

## Scope

The plan lists one modified file: `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx`. The diff contains exactly one modified file, which is that file. The `.claude/specs/elo-rankings/plan.md` modification visible in `git status` is a workflow artefact (the epic-level plan tracking slice completion) and is not feature code — ignored per review rules. No unplanned files were touched.

## Blockers

None.

## Suggestions

#### S1 — `unclaimedTeamsByKey` builds from all teams, not just those in the current replay's placements

- **File:** `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx:451–459`
- **Issue:** The map is guarded by `replay?.placements` being truthy, but it iterates over all `teams` regardless of whether they appear in `replay.placements`. For large leagues with many teams, this is harmless (the lookup at render time is keyed to the placement's own `machine`/`teamName`), but the spec says "filtered client-side to the `(machine, teamName)` pairs present in the replay's placements". The current code filters at lookup time (via the map key) rather than at population time, which is functionally equivalent but a minor spec wording divergence.
- **Fix:** This is not a defect — the map only ever resolves against placement keys, so no spurious buttons can appear. No change is required; noting for awareness.
- **Decision:** Decline

## Nitpicks

#### N1 — IIFE inside `.map` adds nesting depth

- **File:** `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx:662–691`
- **Issue:** The IIFE pattern (`{(() => { ... })()}`) is already at seven levels of indentation inside the placement map. The plan acknowledges this as a deliberate choice over a sub-component. It works and ESLint accepts it; calling it out as a future readability candidate if the pill grows further.
- **Fix:** If the pill ever gains a third element (e.g. a "claimed by" indicator), extract a `PlacementPill` sub-component at that point.
- **Decision:** Accept

## Tests

No new test files were added. The plan explicitly documents this decision (section 12): the placement pill Claim button is not a security or routing invariant, and `TeamsPage.tsx` — the template for this feature — also ships without a dedicated test file. This is consistent with the web component doc's rule that automated tests are required only for "the single enforcement point for a security or routing invariant". No coverage gap exists against the testing strategy.

## Recommended Actions

- **S1** — Decline — Functionally equivalent to the spec's intent; no buttons can appear for non-placement teams. Changing the population loop to filter by placements first would add complexity for no observable benefit.
- **N1** — Decline — The IIFE is the plan's intentional choice; addressing it is deferred to whenever the pill gains a third element.
