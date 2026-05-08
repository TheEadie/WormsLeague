# Review — SPA Scaffolding (Slice 01)

## Verdict

The implementation satisfies all eight acceptance criteria. The project structure, MUI setup, makefile targets, change detection, and CI wiring are all correct. There is one blocker: the reusable CI workflow runs `make web.lint` before `make web.build`, but `web.lint` invokes `npx eslint` and `npx tsc` against local packages (`globals`, `typescript-eslint`, etc.) that only exist after `npm ci` runs — which happens inside `web.build`. On a fresh GitHub Actions runner the Lint step will fail before the Build step installs dependencies.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| `make build` produces bundle under `.artifacts/web/` | MET | `build/web/makefile:4-7` — runs `npm ci` then `vite build`; `vite.config.ts:7` sets `outDir: '../../.artifacts/web/'` |
| `make test` runs ESLint and `tsc --noEmit` | MET | `build/web/makefile:9-14` — `test:: web.test`, `web.test: web.lint`, `web.lint` calls both tools |
| `/` renders a heading identifying the application | MET | `src/App.tsx:4` — `<Typography variant="h1">Worms League</Typography>` |
| Lint violation causes non-zero exit | MET | ESLint configured with `js.configs.recommended`, `tseslint.configs.recommended`, and react-hooks rules (`eslint.config.js`) |
| TypeScript type error causes non-zero exit | MET | `tsconfig.app.json:19` — `strict: true`; `tsc --noEmit` runs in `web.lint` |
| Branch touching web paths triggers CI web job | MET | `zz-detect-changes.yml:70-72` filters `src/Worms.Hub.Web/**` and `build/web/**`; both `code-branch.yml:39-44` and `code-main.yml:39-44` gate `build-web` on that output |
| Branch not touching web paths skips CI web job | MET | Same conditional; only fires when `web-build == 'true'` |
| `CssBaseline` rendered at app root | MET | `src/main.tsx:13` — `<CssBaseline />` inside `ThemeProvider` |

## Scope

The diff matches the plan's Files to Create / Modify table exactly, plus one addition: `src/Worms.Hub.Web/package-lock.json`. This is explained in `learnings.md` — `npm ci` requires a lockfile to exist, so it must be generated and committed. The deviation is fully justified.

## Blockers

#### B1 — CI lint step runs before `npm ci`, causing failure on fresh runner

- **File:** `.github/workflows/zz-build-web.yml:21-26`
- **Issue:** The Lint step (`make web.lint`) executes before the Build step (`make web.build`). `web.lint` calls `npx eslint src` whose config (`eslint.config.js`) imports `globals`, `typescript-eslint`, `eslint-plugin-react-hooks`, and `eslint-plugin-react-refresh` from `node_modules`. On a clean GitHub Actions runner no `node_modules` directory exists yet, so these imports fail and the step errors out before any lint check runs. `npm ci` only runs inside `web.build`, which comes after.
- **Fix:** Swap the Lint and Build steps in `zz-build-web.yml` so Build (which runs `npm ci`) precedes Lint.
- **Decision:** — *(pending)*

## Suggestions

#### S1 — Remove `fetch-depth: 0` from the web CI workflow

- **File:** `.github/workflows/zz-build-web.yml:14-16`
- **Issue:** A full git clone (`fetch-depth: 0`) is only needed when a job inspects commit history (e.g. change detection, version tagging). A build + lint job needs only the working tree.
- **Fix:** Remove the `with: fetch-depth: 0` block from the Checkout step, allowing it to use the default shallow clone.
- **Decision:** — *(pending)*

## Nitpicks

*(none)*

## Tests

No test code was added. The spec explicitly places unit and component tests out of scope for this slice, requiring only lint and type-check. Coverage is appropriate for a scaffolding slice.

## Recommended Actions

- **B1** — Accept — Swapping two steps in the YAML is a one-line fix and without it the CI web job will never pass on a clean runner.
- **S1** — Accept — Shallow clone is faster and the full history serves no purpose in this job; consistent with keeping CI lean.
