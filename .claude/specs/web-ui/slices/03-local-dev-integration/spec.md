# Local-dev Integration

## Overview

The SPA joins the existing `docker compose up` stack so the full Hub can be exercised locally end-to-end. Running `docker compose up` starts a web service that builds and serves the SPA's static bundle, making it reachable from a host browser.

## Requirements

- A Dockerfile exists for the web service that builds the SPA's static bundle and serves it via a static file server (e.g. nginx).
- `docker-compose.yaml` includes a web service that builds from that Dockerfile and exposes the SPA on host port `3000`.
- The web service starts cleanly as part of `docker compose up` alongside all existing services.
- The existing placeholder page is served correctly at `http://localhost:3000`.
- All existing services (`azure-storage`, `database`, `flyway-init`, `hub-gateway`, `hub-worker`, `hub-wa-runner`) continue to start and behave as before.

## Out of Scope

- Hot reload or a Vite dev server mode — the static bundle is always served.
- The SPA communicating successfully with the gateway — CORS configuration is a later slice.
- Authentication or sign-in — a later slice.
- A real landing page — a later slice (the placeholder is acceptable).
- Production Docker image packaging, Docker Hub publishing, or `make` targets for building/releasing the web Docker image — deferred to the production deployment slice.

## Acceptance Criteria

- Given a working Docker environment, when `docker compose up` is run from the repo root, then all services including the new web service reach a running/healthy state without errors.
- Given the stack is up, when a browser opens `http://localhost:3000`, then the SPA's placeholder page loads without errors.
- Given the stack is up, when each existing service is exercised as before (gateway reachable at `http://localhost:5005`, storage emulator at ports `10000–10002`, database at `5432`), then their behaviour is unchanged.
- Given `make build` is run, then it completes successfully (the web Dockerfile's addition does not break existing make targets).
