# Review — Public Landing Page

## Verdict

The implementation satisfies every acceptance criterion in the spec. All new components match the plan exactly (with the SVG/PNG substitution explicitly permitted by the plan), `make web.build` and `make web.lint` both pass cleanly, and the image is committed as a static asset. There are no blockers. Two minor suggestions follow — one around image sizing and one around a missing licence comment in the SVG itself — along with a nitpick on the `gap` + `Stack spacing` duplication in `LandingPage.tsx`.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Navigating to `/` renders the landing page with worm image, "Worms Hub" heading, and Sign In button | MET | `LandingPage.tsx` renders all three in a centred Stack under the `/` index route in `App.tsx` |
| Clicking the Sign In button produces no navigation and no visible error | MET | `Button` in `LandingPage.tsx:24` has no `onClick`, `href`, or form action |
| The header is visible on the landing page and displays the app name/branding | MET | `Header.tsx` renders `AppBar` + `Toolbar` with "Worms Hub" Typography; `Layout.tsx` includes it on every page |
| The footer is visible on the landing page and contains a copyright statement | MET | `Footer.tsx:8` renders `© {new Date().getFullYear()} Worms Hub` |
| The worm image is committed as a static asset (not fetched from an external URL) | MET | `src/Worms.Hub.Web/public/worm.svg` is an untracked new file (will be committed); `LandingPage.tsx:20` references `/worm.svg` |
| `make web.build` completes with no errors | MET | Build output shows `✓ built in 545ms` with no errors |
| `make web.lint` passes (ESLint, TypeScript type-check, Prettier) with no errors | MET | Output shows `All matched files use Prettier code style!`; ESLint and `tsc --noEmit` both exited clean |

## Scope

The diff matches the plan's Files to Create / Modify table in full:

| Plan entry | Present in diff? | Note |
|---|---|---|
| `src/Worms.Hub.Web/public/worm.png` | Yes (as `worm.svg`) | Plan explicitly permitted SVG; learnings.md explains the substitution |
| `src/Worms.Hub.Web/src/components/Header.tsx` | Yes | New untracked file |
| `src/Worms.Hub.Web/src/components/Footer.tsx` | Yes | New untracked file |
| `src/Worms.Hub.Web/src/components/Layout.tsx` | Yes | New untracked file |
| `src/Worms.Hub.Web/src/pages/LandingPage.tsx` | Yes | New untracked file |
| `build/web/nginx.conf` | Yes | New untracked file |
| `src/Worms.Hub.Web/package.json` | Yes | `react-router: ^7.15.0` added to dependencies |
| `src/Worms.Hub.Web/package-lock.json` | Yes | Regenerated with `react-router` entry |
| `src/Worms.Hub.Web/src/App.tsx` | Yes | Replaced placeholder heading with router tree |
| `src/Worms.Hub.Web/index.html` | Yes | Title changed from "Worms League" to "Worms Hub" |
| `build/web/Dockerfile` | Yes | `COPY build/web/nginx.conf /etc/nginx/conf.d/default.conf` added |

No files outside the plan were changed (excluding the process artefact `.claude/specs/web-ui/plan.md`, which ticked the slice as complete — that is a workflow artefact, not a feature change).

## Blockers

None.

## Suggestions

#### S1 — Image is rendered at its intrinsic size when viewport is narrow

- **File:** `src/Worms.Hub.Web/src/pages/LandingPage.tsx:20`
- **Issue:** `maxWidth: 300` is a fixed pixel limit. On viewports narrower than 300 px (some mobile widths) the image will overflow its container rather than shrink.
- **Fix:** Add `width: '100%'` alongside `maxWidth: 300` so the image scales down on small screens: `style={{ maxWidth: 300, width: '100%', height: 'auto' }}`.
- **Decision:** Accept

#### S2 — Licence comment lives only in `LandingPage.tsx`, not in the SVG itself

- **File:** `src/Worms.Hub.Web/public/worm.svg:2`
- **Issue:** The plan asks for the image source and licence to be recorded in `LandingPage.tsx` (done), but the SVG already contains an inline comment (`<!-- Original artwork, created for Worms Hub. No licence restrictions. -->`). The `LandingPage.tsx` comment says the same thing at line 1. Having both is fine, but neither references the other — anyone looking at the SVG alone sees the claim, and anyone looking at the page component sees the claim, but there is no cross-reference. This is low-priority; it's just a traceability note.
- **Fix:** Optionally add a short note in `LandingPage.tsx:1` like `// Image source: public/worm.svg — original artwork, no licence restrictions` to make it explicit that the asset lives in the repo.
- **Decision:** Decline

## Nitpicks

#### N1 — `gap: 3` on the outer `Box` and `spacing={3}` on the inner `Stack` are redundant

- **File:** `src/Worms.Hub.Web/src/pages/LandingPage.tsx:14,19`
- **Issue:** The outer `Box` sets `gap: 3` as part of its flexbox layout, but the only child of that `Box` is a single `Stack`. The `Stack` itself already adds `spacing={3}` between its children. The outer `gap` has no visual effect because there is only one flex child. It's dead CSS.
- **Fix:** Remove `gap: 3` from the outer `Box`'s `sx` prop.
- **Decision:** Accept

## Tests

No test coverage was added or changed by this slice. This is appropriate — the slice introduces only UI components (React, MUI) with no logic that warrants unit tests. The spec does not require any tests, and the testing strategy confirms there is no expectation for React component unit tests at this stage of the project. No coverage gaps or padding tests to flag.

## Recommended Actions

- **S1** — Accept — The `width: '100%'` fix is a one-line change that prevents image overflow on narrow viewports; it is a straightforward quality improvement.
- **S2** — Decline — Both locations already carry the attribution; adding a cross-reference is cosmetic bookkeeping that adds no real value given both files are in the same repo.
- **N1** — Accept — Removing the dead `gap: 3` eliminates a misleading property with no visual effect. One-line fix.
