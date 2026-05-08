# Learnings: Web Linting in CI

## Implementation Notes

### `@microsoft/eslint-formatter-sarif` installs a transitive eslint v8

When `npm install @microsoft/eslint-formatter-sarif@^3.1.0` runs, npm also installs `eslint@8.57.1`
as a transitive dependency (the package's own dep tree includes eslint v8). This triggers deprecation
warnings during `npm install` but does not affect the running behaviour: the CLI entrypoint picked
up by `npx eslint` is still v9 (the top-level project dep), and the formatter is loaded correctly
by ESLint v9 at runtime. The SARIF file is produced cleanly with 0 results on a clean source tree.

No action required; the warnings are cosmetic noise from the npm install step.

### SARIF formatter verified locally before wiring CI

The plan described the CI job structure but did not call for a local verification step before
committing. Running `make web.build && cd src/Worms.Hub.Web && npx eslint src --format @microsoft/eslint-formatter-sarif --output-file eslint.sarif` confirmed the formatter works with the flat config (ESLint v9) and produces valid SARIF (`runs: 1, results: 0`). The test file was removed before committing.

### `make web.build` replaced with targeted steps per job

The plan specified `make web.build` in every linting job. Post-review, this was refined:

- **CodeQL (`build-mode: none`)**: CodeQL's JS/TS extractor works directly on source files and
  needs neither Node nor `node_modules`. The Setup Node and Build steps were removed entirely.
- **ESLint and Prettier**: These execute Node packages from `node_modules` and do need `npm ci`,
  but not the `vite build` that `make web.build` also runs. The Build step was replaced with an
  inline `cd src/Worms.Hub.Web && npm ci` step, avoiding the wasted bundle compilation.

No new make target was added; the `npm ci` command is inlined directly in the CI steps.

## Files Added (not in plan)

None.
