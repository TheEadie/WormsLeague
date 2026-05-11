# Review — Mockup Alignment

## Verdict

The implementation matches the spec and the plan very closely. The landing page, header, and footer have been restyled per the mockups, JetBrains Mono is loaded via Fontsource with a `THIRD_PARTY_NOTICES.md` attribution, and the theme has been extracted to a shared `theme.ts`. `make web.build` and `make web.lint` (ESLint, `tsc --noEmit`, Prettier) all pass cleanly. No blockers — only minor process-level observations.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Wide viewport: hero left, sign-in card right | MET | `LandingPage.tsx:13` — `gridTemplateColumns: { xs: '1fr', md: '1.15fr 0.85fr' }` |
| Narrow viewport: stacks vertically, hero on top | MET | `LandingPage.tsx:13` — `xs: '1fr'` with hero column declared first in DOM order |
| Hero column contains worm image above headline and the headline "Every shot. Every kill. Archived." — nothing else | MET | `LandingPage.tsx:28-44` — only `<Box component="img">` and the `<Typography variant="h2">` headline |
| Sign-in card contains heading, body text, button, "League access" divider, one info row — nothing else | MET | `LandingPage.tsx:56-83` — exactly heading, body, button, divider, single Stack row |
| Sign-in button is non-functional | MET | `LandingPage.tsx:65-67` — `<Button>` has no `onClick` |
| Header shows worm image, "Worms Hub" wordmark, colour-scheme picker with mockup styling | MET | `Header.tsx:9-22` — sticky AppBar, paper bg, bottom divider, `<Brand/>`, `<ColourSchemePicker/>` |
| Footer shows existing copyright styled consistently with new chrome | MET | `Footer.tsx:6-18` — top border, body2 + `text.secondary` |
| Renders correctly under both light and dark MUI palettes | MET | All colours use palette-aware tokens (`divider`, `background.paper`, `background.default`, `text.secondary`); `theme.ts` keeps the `colorSchemes: { light: true, dark: true }` config and CSS-variables selector from slice 06 |
| JetBrains Mono loaded and applied to accent typography, with required attribution | MET | `main.tsx:3-5` imports weights 400/500/700; `theme.ts:13-17` re-skins `overline` to use mono stack; `Brand.tsx:17` reuses `monoFontFamily`; `THIRD_PARTY_NOTICES.md` provides OFL 1.1 attribution |

## Scope

The diff matches the plan's "Files to Create / Modify" table exactly:

- New: `src/theme.ts`, `src/components/Brand.tsx`, `THIRD_PARTY_NOTICES.md` — all present.
- Modified: `package.json`, `package-lock.json`, `main.tsx`, `LandingPage.tsx`, `Header.tsx`, `Footer.tsx` — all present.
- No files outside the plan were touched (the `plan.md` modification and untracked slice directory are workflow artefacts and ignored per the review rules).

Implementations of `Brand.tsx`, `theme.ts`, `Header.tsx`, `Footer.tsx`, `main.tsx`, and `LandingPage.tsx` match the plan's code blocks effectively verbatim (Prettier reflowed the `<Paper>` opener as the learnings note; no semantic deviation).

## Blockers

None.

## Suggestions

### S1 — Dependency version drift vs the plan

- **File:** `src/Worms.Hub.Web/package.json:15`
- **Issue:** The plan specifies exact version `^5.2.5` for `@fontsource/jetbrains-mono`, but the committed `package.json` has `^5.2.8` and the lockfile resolved `5.2.8`. The caret range is functionally compatible, but the deviation is undocumented in `learnings.md`.
- **Fix:** Either pin to `^5.2.5` to match the plan, or add a one-line note in `learnings.md` that `npm install @fontsource/jetbrains-mono` picked up the current 5.2.8 patch at install time.
- **Decision:** Accept

## Nitpicks

### N1 — `theme.ts` exports the same value twice

- **File:** `src/Worms.Hub.Web/src/theme.ts:21-22`
- **Issue:** Both a named `export { theme }` and `export default theme` are emitted; only the default is consumed (`main.tsx:9` `import theme from './theme.ts'`). The named export is dead code and risks two import shapes for the same value, which can drift.
- **Fix:** Drop the named `theme` export and keep only `export { monoFontFamily }` plus `export default theme`. This matches how `Brand.tsx` already only consumes `monoFontFamily`.
- **Decision:** Accept

### N2 — `Footer.tsx` uses `px: 3` not in the plan

- **File:** `src/Worms.Hub.Web/src/components/Footer.tsx:8`
- **Issue:** The plan's `Footer.tsx` block has only `py: 2`; the implementation also adds `px: 3`. This is harmless cosmetics for a centred body2 line, but is a small deviation from the plan that isn't called out in `learnings.md`.
- **Fix:** Either drop `px: 3` or note the addition. No functional impact.
- **Decision:** Accept

## Tests

This slice ships no automated tests. The web project currently has no test setup at all, and the testing-strategy doc focuses on .NET (NUnit) projects; the spec and plan did not require new tests, and the acceptance criteria are visual/structural. No coverage gap to call out at this slice's bar. Manual verification steps in `plan.md` §Verification cover the behaviour adequately.

## Recommended Actions

- **S1** — Accept — A 30-second `learnings.md` note (or a pin) keeps the audit trail honest; cheap and avoids confusion if 5.2.x has a future patch that breaks the wordmark rendering.
- **N1** — Accept — Removing the unused named export is a one-line cleanup that prevents the file from offering two redundant import shapes for the same value.
- **N2** — Decline — `px: 3` is harmless on a centred footer, the diff is trivial, and chasing every micro-deviation from the plan adds churn without benefit.
