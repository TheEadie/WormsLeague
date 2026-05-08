# SPA Scaffolding

## Overview

A new React/TypeScript single-page application project is added to the repository under `src/`, builds and lints cleanly, includes MUI as the styling framework, renders a placeholder home page, and is wired into the makefile and CI alongside the existing .NET projects.

## Requirements

- A new React/TypeScript project using Vite exists under `src/Worms.Hub.Web/`.
- The project uses npm as its package manager.
- ESLint and Prettier are configured and enforce consistent code style across the project.
- MUI (`@mui/material`) is installed and its baseline setup (theme provider, CSS baseline) is applied at the app root.
- The app renders a placeholder home page visible at `/` with at minimum a heading identifying the application (e.g. "Worms League"). No functional content is required.
- `make build` includes building the SPA (installs dependencies, compiles TypeScript, produces a production bundle).
- `make test` includes linting the SPA via ESLint and type-checking via `tsc --noEmit`.
- A new `build/web/makefile` defines the SPA-specific targets (`web.build`, `web.test`, `web.lint`) following the same pattern as `build/cli/makefile`.
- CI runs the new SPA build and lint steps on every branch push and on every PR, alongside the existing Hub and CLI jobs, gated on changes detected under `src/Worms.Hub.Web/**` and `build/web/**`.

## Out of Scope

- Local-dev Docker Compose integration (covered in the next slice, Local-dev integration).
- Any real page content, routing, or navigation beyond the single placeholder home page.
- Authentication or calls to the Hub Gateway API.
- CORS configuration on the Gateway.
- Deployment or containerisation of the SPA.
- Unit or component tests — this slice only requires lint and type-check to pass; a testing framework (e.g. Vitest) can be added in a later slice when there is something to test.
- Any CI job for releasing or publishing the SPA artifact.

## Acceptance Criteria

- Given the repository is checked out with Node and npm available, running `make build` completes without error and produces a production bundle under `.artifacts/web/` (or equivalent artifacts directory consistent with the existing `.artifacts/` layout).
- Given the repository is checked out, running `make test` runs ESLint and `tsc --noEmit` against the SPA source and exits non-zero if any lint or type errors are present.
- Given the production bundle is served statically, navigating to `/` renders a page with a heading that identifies the application.
- Given a source file contains a lint violation, `make test` (or `web.lint` directly) exits non-zero and reports the violation.
- Given a source file contains a TypeScript type error, `make test` exits non-zero and reports the error.
- Given a branch push or PR that touches `src/Worms.Hub.Web/**` or `build/web/**`, the CI pipeline runs the web build and lint job and reports its result.
- Given a branch push or PR that does not touch any web-related paths, the CI pipeline does not run the web build and lint job.
- MUI's `CssBaseline` component is rendered at the app root so that MUI's baseline styles are applied globally.

## Open Questions

(none)
