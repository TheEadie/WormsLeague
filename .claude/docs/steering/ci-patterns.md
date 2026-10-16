# CI Patterns

Conventions for how the GitHub Actions workflows in this repo are structured. Follow these when adding or modifying CI jobs.

## Build vs release: always build, gate release only

Build jobs always run on every push. Change detection outputs (from `zz-detect-changes.yml`) are used only to gate **release or deploy** jobs, not build jobs.

In the orchestrator workflows (`code-branch.yml`, `code-main.yml`):

- `build-hub`, `build-cli`, `build-web` — no `if:` condition; always run
- `release-hub`, `release-cli`, and any future deploy jobs — gated with `if:` on the relevant change detection output

This means a component's build is validated on every push regardless of what changed. Change detection controls whether artefacts are published or deployed, not whether the build runs at all.

When adding a new component to CI, wire its build job unconditionally and add the change detection `if:` to the corresponding release/deploy job only.

### Exception: integration test jobs

Slow integration test jobs that need Docker (or other provisioned infrastructure) may be gated on a dedicated per-component test change-detection output, rather than always running. Examples:

- `wa-runner-test` gates "WA Runner - Integration Tests"
- `gateway-integration-test` gates "Gateway - Integration Tests" (the Storage repository contract tests)

The test output combines the component's source paths, its test project path, any infrastructure the tests depend on (e.g. `src/database/migrations/**`, `docker-compose.yaml`), and `hub-common`. It is passed into `zz-build-hub.yml` as an input and used in the job's `if:`. A skipped job does not fail the overall build gate; a failed one does. Test-only paths are never added to release/deploy filters.

## Linting belongs in Code Scanning, not the build

Linting and static analysis (ESLint, Prettier, `tsc --noEmit`, Roslyn analysers) belong in `code-scanning.yml` as SARIF uploads. They are not build steps.

- Do not add lint steps to `zz-build-*.yml` workflows
- Do not add lint targets to `make test` or wire them into the build
- `make test` means running actual tests — unit or integration

The rationale is consistency with how .NET linting works in this repo (Roslyn/JetBrains runs only in code-scanning, never in the build CI), and to keep build feedback fast and focused.

## Web CI: build before lint

In any CI job that runs `make web.lint`, the `make web.build` step must come first. `web.build` runs `npm ci`, which installs `node_modules`. The linting tools (`eslint`, `tsc`, `prettier`) are local packages in `node_modules` — they do not exist on a fresh runner before `npm ci` runs.

Correct order in a web CI job:
1. Checkout
2. Setup Node
3. `make web.build` (runs `npm ci` + Vite build)
4. `make web.lint` (runs ESLint, tsc, Prettier — all require node_modules)

## Code-scanning jobs: `npm ci` only, not `make web.build`

Jobs in `code-scanning.yml` that run ESLint or Prettier need `node_modules` to be present, but they do not need a compiled bundle. These jobs must call `npm ci` directly rather than `make web.build` (which also runs a full Vite compilation that is immediately discarded).

- ESLint SARIF and Prettier jobs: inline `npm ci`, then the tool invocation
- CodeQL with `build-mode: none`: needs neither Node setup nor `npm ci`
- Only the dedicated `zz-build-web.yml` job (which uploads the bundle artefact) needs `make web.build`

## `fetch-depth: 0` only when git history is needed

The default `actions/checkout` does a shallow clone, which is sufficient for build and lint jobs. Only add `fetch-depth: 0` to a job that actually inspects git history — change detection (`zz-detect-changes.yml`), version tagging, or changelog generation. Build and test jobs do not need the full history.
