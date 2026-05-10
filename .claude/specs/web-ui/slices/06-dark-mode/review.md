# Review — Dark Mode

## Verdict

The implementation satisfies all acceptance criteria. MUI's CSS-variables colour-scheme engine is wired correctly in `main.tsx`, `ColourSchemePicker` exposes the three-way toggle with accessible markup, and `Header` renders it in the right slot. ESLint, `tsc --noEmit`, and Prettier all pass clean. No blockers. One minor suggestion and one nitpick are noted below.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| No stored preference + OS dark → dark palette on load | MET | `defaultMode="system"` on `ThemeProvider` (main.tsx:14); MUI reads `prefers-color-scheme` when no `localStorage` key exists |
| No stored preference + OS light → light palette on load | MET | Same mechanism |
| Click sun icon → light palette applied immediately; "light" written to localStorage | MET | `onClick={() => setMode('light')}` (ColourSchemePicker.tsx:15); MUI writes `mui-mode` automatically |
| Click moon icon → dark palette applied immediately; "dark" written to localStorage | MET | `onClick={() => setMode('dark')}` (ColourSchemePicker.tsx:36) |
| Click system icon → palette matches OS; localStorage updated | MET | `onClick={() => setMode('system')}` (ColourSchemePicker.tsx:25) |
| "light" in localStorage + OS dark → light palette wins | MET | MUI's `useColorScheme` honours the stored key over the OS preference |
| "system" in localStorage + OS scheme change → palette updates without reload | MET | MUI's CSS-variables engine wires a `prefers-color-scheme` media listener; no manual listener needed |
| Header shows toggle; active state visually distinct | MET | `color={mode === 'x' ? 'inherit' : 'default'}` (ColourSchemePicker.tsx:16, 27, 38); `aria-pressed` also set |
| Landing page readable in both palettes | MET | MUI built-in palettes are used; no custom colours introduced that could break either mode |

## Scope

The diff matches the plan's Files to Create / Modify table exactly:

- `src/Worms.Hub.Web/src/components/ColourSchemePicker.tsx` — created as planned
- `src/Worms.Hub.Web/src/main.tsx` — updated theme as planned
- `src/Worms.Hub.Web/src/components/Header.tsx` — placeholder comment replaced with `<ColourSchemePicker />` as planned
- `src/Worms.Hub.Web/package.json` — `@mui/icons-material@^9.0.1` added to `dependencies` as planned
- `src/Worms.Hub.Web/package-lock.json` — regenerated as planned

No files changed outside the plan. No planned file is absent. `learnings.md` reports no deviations.

## Blockers

None.

## Suggestions

#### S1 — Guard against `mode === undefined` for initial render

- **File:** `src/Worms.Hub.Web/src/components/ColourSchemePicker.tsx:16, 27, 38`
- **Issue:** `useColorScheme()` returns `mode` typed as `'light' | 'dark' | 'system' | undefined`. On first render before MUI has resolved the stored/OS preference, `mode` is `undefined`, which means all three buttons will show as inactive simultaneously (no button matches `undefined`). The plan notes this is a pure-client SPA so `undefined` "will not appear in practice" — and `tsc` accepts this because MUI types the comparisons as safe — but the scenario can briefly occur between React's first paint and the colour-scheme hook settling.
- **Fix:** A simple `if (!mode) return null` or defaulting the active-state expression to also match `undefined` for the system button (e.g. `color={mode === 'system' || mode === undefined ? 'inherit' : 'default'}`) would eliminate the flash. Alternatively, document the conscious decision in a comment. This is not a spec violation — the spec does not address the sub-frame hydration window.
- **Decision:** Accept

## Nitpicks

#### N1 — `web.md` component doc records MUI v7; actual dependency is v9

- **File:** `.claude/docs/components/web.md:8`
- **Issue:** The doc says "Material UI (MUI v7)" but `package.json` has `@mui/material: ^9.0.1`. This is a pre-existing staleness, not introduced by this diff, and is out of scope for this review. Noting it here because the dark-mode plan correctly cited v9, making the mismatch more visible.
- **Fix:** Update `.claude/docs/components/web.md` to reflect MUI v9 in a separate commit or as part of a doc-refresh pass.
- **Decision:** Accept

## Tests

No tests were added. The plan explicitly documents this decision: the slice is pure UI composition wiring MUI's built-in colour-scheme system; there are no pure functions to unit-test, and a test that asserts `setMode` was called would be asserting mock setup. This is consistent with the testing-strategy guidance. No gap.

## Recommended Actions

- **S1** — Decline — The plan's rationale (pure-client SPA, undefined not observable in practice) is sound, and MUI's CSS-variables mode resolves synchronously from `localStorage` before the first paint in the majority of browsers. Addressing it would add complexity not asked for by the spec.
- **N1** — Accept — The doc should match reality; update `web.md` in a small follow-up or doc-refresh pass.
