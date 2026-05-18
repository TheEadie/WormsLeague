# Review — ELO Leaderboard on League Cards

## Verdict

The implementation satisfies every acceptance criterion in the spec. The diff is limited to `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx` (plus an unrelated tick in the epic plan), `make web.lint` and `make web.build` both pass cleanly, and the `LeagueDetailPage.tsx` standings table is untouched. No blockers. A few small suggestions and nitpicks around React keys, the duplicated `StandingDto` interface, and a minor visual concern are listed below for triage.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Card with 3+ rated players renders 3 rows + `top 3 of M` | MET | `LeagueListPage.tsx:115-117` (`top {Math.min(3, …)} of {length}`), `:120` (`.slice(0, 3)`) |
| Card with 2 rated players: 2 rows + `top 2 of 2` | MET | Same computation at `:115-117`; `.slice(0, 3)` returns 2 |
| Card with 1 rated player: 1 row + `top 1 of 1` | MET | Same computation; `.slice(0, 3)` returns 1 |
| Card with empty standings: section omitted entirely | MET | Guard at `LeagueListPage.tsx:86` (`length > 0`) |
| Card with null standings: section omitted entirely | MET | Guard at `LeagueListPage.tsx:86` (`!== null`) |
| Rank medal colours gold/silver/bronze | MET | `LeagueListPage.tsx:122` (`['#ffca28', '#bdbdbd', '#cd7f32']`) applied via `color: medal` at `:140` |
| ELO uses `monoFontFamily`, right-aligned | MET | `LeagueListPage.tsx:159` (`fontFamily: monoFontFamily`) and `:163` (`textAlign: 'right'`) |
| Long player names truncate with ellipsis | MET | `LeagueListPage.tsx:150-152` (`overflow: hidden`, `textOverflow: ellipsis`, `whiteSpace: nowrap`) |
| Card click target covers leaderboard area | MET | Entire card still wrapped in `<Link>` at `:33`; no per-row handlers added |
| Existing card content unchanged | MET | id badge (`:49-71`), name (`:73-75`), scheme chip (`:77-84`) untouched in diff |
| Detail page unchanged | MET | `LeagueDetailPage.tsx` not in diff |
| No API changes | MET | No backend files in diff |
| `make web.lint` and `make web.build` pass | MET | Verified locally: lint completes with "All matched files use Prettier code style!" and ESLint/tsc clean; `vite build` produces `.artifacts/web/` bundle |

## Scope

Plan listed exactly one file to modify: `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx`. The diff modifies that one file plus `.claude/specs/elo-rankings/plan.md` (a workflow artefact — an epic-level status tick) and adds the slice's spec/plan/learnings directory (workflow artefacts, excluded by the review rules). No source files outside the plan were touched. Imports added (`Divider`, `Stack`) match the plan; `monoFontFamily` was already imported. The leaderboard JSX block matches the plan's design 1:1 including the `as const` cast for medal colours.

## Blockers

None.

## Suggestions

### S1 — Duplicate `StandingDto` interface

- **File:** `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx:17-21`
- **Issue:** `StandingDto` is now declared identically in both `LeagueListPage.tsx` (`:17-21`) and `LeagueDetailPage.tsx` (`:23-27`). The two pages now drift independently if the API shape changes, and the spec itself notes the new declaration must match the existing one.
- **Fix:** Extract `StandingDto` (and arguably `LeagueDto`) into a shared module, e.g. `src/Worms.Hub.Web/src/api/types.ts`, and import from both pages. Out of scope for the slice's stated scope (UI-only on the list page) but worth doing as a tiny follow-up.
- **Decision:** Accept

## Nitpicks

### N1 — `index` used as React key for rendered rows

- **File:** `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx:127`
- **Issue:** `<Box key={index} …>` keys rows by array position. The rows are always rendered in API order and the list is at most 3 entries, so this is harmless in practice, but `playerName` would be a more stable key and matches the convention used elsewhere in the codebase for keyed lists of player rows.
- **Fix:** `key={s.playerName}` (or `key={`${s.playerName}-${index}`}` if a duplicate-name guard is wanted).
- **Decision:** Accept

### N2 — `medal` is `undefined` for any 4th+ row (defence-in-depth only)

- **File:** `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx:122-124`
- **Issue:** `(['#ffca28', '#bdbdbd', '#cd7f32'] as const)[index]` returns `undefined` for `index >= 3`. The `.slice(0, 3)` at `:120` guarantees we never reach that branch, so this is purely theoretical, but the TypeScript type of `medal` is `'#ffca28' | '#bdbdbd' | '#cd7f32' | undefined` and downstream the `color` sx accepts `undefined`. A future change to the slice or row count could silently degrade.
- **Fix:** Either tighten by indexing `place - 1` with a non-null assertion explicitly tied to the `slice(0, 3)` bound, or pull the array out as `const MEDAL_COLOURS: readonly [string, string, string] = …` and read with a bounded helper. Not worth doing on its own.
- **Decision:** Decline

### N3 — Caption text rendered as `Leaderboard` and uppercased via CSS

- **File:** `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx:101-106`
- **Issue:** The spec says the caption reads `LEADERBOARD`. The DOM text node is `Leaderboard` and `textTransform: 'uppercase'` does the case change at render time. This is functionally identical visually and was explicitly called out in the plan, but means copy/paste of the rendered text and accessibility tools will see the title case form rather than uppercase. Consistent with the rest of the codebase's caption pattern, so likely acceptable.
- **Fix:** None needed unless the team prefers literal uppercase strings for accessibility/copy parity.
- **Decision:** Accept

## Tests

No tests were added or modified, consistent with the slice scope and `learnings.md`. The web component layer for these list/card pages currently has no Vitest coverage (confirmed by repo grep — no `LeagueListPage.test.tsx`), and the slice did not commit to introducing test infrastructure. Per `.claude/docs/components/web.md`, this is acceptable because the leaderboard is a purely visual list-card concern, not a security/routing invariant. No coverage gaps to flag; no padding or fragile tests were introduced.

## Recommended Actions

- **S1** — Decline — explicitly out of scope; the existing duplication is small (one interface, three fields), and consolidating is better tracked as a follow-up rather than expanding this slice.
- **N1** — Accept — trivial change to a more stable key, and `playerName` is unique-by-construction in a single-league standings array.
- **N2** — Decline — defended by `.slice(0, 3)` immediately above; refactoring for a hypothetical regression isn't worth the noise.
- **N3** — Decline — pattern is consistent with the rest of the design and was deliberate in the plan.
