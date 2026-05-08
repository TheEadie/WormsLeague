# Review — Web Linting in CI

## Verdict

The implementation satisfies all seven acceptance criteria cleanly. The three new Code Scanning jobs (`codeql-javascript-typescript`, `eslint`, `prettier`) are correctly structured, follow the patterns set by existing jobs, and `actions-timeline` has been updated to depend on all three. `make web.build` precedes every lint step as required. Quality checks pass locally: `make web.lint` exits clean, and the ESLint SARIF formatter produces valid output (verified during review). No blockers; ready to merge.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| CodeQL job fails and uploads SARIF on a JS/TS issue | MET | `codeql-javascript-typescript` job at `code-scanning.yml:132`; `analyze@v4` handles SARIF upload automatically |
| ESLint job fails and uploads SARIF on a lint violation | MET | ESLint exits non-zero on violations; `upload-sarif` step uses `if: always()` at `code-scanning.yml:168` |
| Prettier job fails on a formatting violation | MET | `npx prettier --check src` exits non-zero; `code-scanning.yml:186` |
| `actions-timeline` depends on all three new jobs | MET | `needs` list at `code-scanning.yml:191` includes all seven scan jobs |
| All three jobs pass on a clean source tree | MET | `make web.lint` exits 0; ESLint SARIF formatter verified locally (0 results) |
| Each job sets up Node 22.x and runs `make web.build` before linting | MET | `actions/setup-node@v4` with `node-version: 22.x` and `make web.build` precede lint steps in all three jobs |
| `build-web` job in `zz-build-web.yml` is named `Build` | MET | `zz-build-web.yml:10` |

## Scope

The diff matches the plan's "Files to Create / Modify" table exactly:

| File | Plan | Diff |
|---|---|---|
| `.github/workflows/zz-build-web.yml` | Rename job | ✓ Renamed |
| `.github/workflows/code-scanning.yml` | Three new jobs + `actions-timeline.needs` update | ✓ All present |
| `src/Worms.Hub.Web/package.json` | Add `@microsoft/eslint-formatter-sarif` | ✓ Added to `devDependencies` |
| `src/Worms.Hub.Web/package-lock.json` | Regenerate | ✓ Regenerated |

No files outside the plan were changed. `learnings.md` notes the transitive eslint v8 deprecation warnings from `npm install` — no action needed as the v9 entrypoint is used at runtime and SARIF output is confirmed clean.

## Blockers

None.

## Suggestions

None.

## Nitpicks

None.

## Tests

No test code added or changed. This slice is pure CI configuration; the verification in `learnings.md` (local SARIF run confirming `runs: 1, results: 0`) is the appropriate substitute for automated test coverage here.

## Recommended Actions

No findings to act on.
