# Web Linting in CI

## Overview

`code-scanning.yml` gains three new jobs — CodeQL (JavaScript/TypeScript), ESLint, and Prettier — covering the web SPA, consistent with the existing Roslyn and JetBrains jobs, and included in the `actions-timeline` aggregator.

## Requirements

- The `build-web` job in `zz-build-web.yml` is renamed from `Build and lint` to `Build`, reflecting that linting does not occur in the build workflow and is consistent with the naming of other component build jobs.
- `code-scanning.yml` includes a job that runs CodeQL analysis for JavaScript and TypeScript against the SPA source.
- `code-scanning.yml` includes a job that runs ESLint against the SPA source and uploads results as SARIF.
- `code-scanning.yml` includes a job that runs Prettier format-checking against the SPA source.
- In each web linting job, `npm ci` (via `make web.build`) runs before the lint step so that local packages are available.
- All three new jobs are added to the `needs` list of the existing `actions-timeline` job.
- The new jobs run on every push to `main`, every PR targeting `main`, and on the existing scheduled run — consistent with the trigger set already defined in `code-scanning.yml`.
- The CodeQL job uses the same `github/codeql-action` actions already used by the existing CodeQL jobs.
- ESLint results are uploaded as SARIF using `github/codeql-action/upload-sarif`, consistent with the Roslyn and JetBrains jobs.
- Prettier does not produce SARIF; its job reports a failure by exiting non-zero when formatting violations are found.

## Out of Scope

- Adding lint steps to `zz-build-web.yml` or any other build workflow (beyond the rename above).
- Wiring `web.lint` into `make test`.
- TypeScript type-checking (`tsc --noEmit`) as a separate code-scanning job — it is already covered as part of `make web.lint` and will run alongside ESLint and Prettier in CI where invoked.
- Change-detection gating on the new code-scanning jobs — the existing workflow triggers apply uniformly to all jobs in `code-scanning.yml`.
- Modifying the ESLint or Prettier configuration beyond what is needed to emit SARIF output.

## Acceptance Criteria

- Given a PR that introduces a TypeScript or JavaScript file with a CodeQL-detectable issue, the `CodeQL (javascript-typescript)` job in `code-scanning.yml` fails and uploads a SARIF report.
- Given a PR that introduces an ESLint violation, the ESLint job in `code-scanning.yml` fails and a SARIF report is uploaded and visible in the repository's Security tab.
- Given a PR that introduces a Prettier formatting violation, the Prettier job in `code-scanning.yml` fails.
- Given all web linting jobs pass, the `actions-timeline` job completes successfully (it depends on all three new jobs in addition to the pre-existing ones).
- Given a clean SPA source with no violations, all three new jobs pass on a push to `main`.
- Each new job sets up Node 22.x and runs `make web.build` before any lint step.
- The `build-web` job in `zz-build-web.yml` is named `Build` (not `Build and lint`).
