# Production Deployment

## Overview

The SPA is bundled into the Gateway Docker image and served directly from the Gateway at `worms.davideadie.dev`. This avoids new Azure resources, eliminates CORS concerns, and reuses the existing container release pipeline.

## Requirements

- The Gateway Dockerfile is extended with a Node build stage that compiles the SPA. The compiled assets are copied into the .NET publish output so the Gateway serves them as static files.
- `VITE_GATEWAY_URL` is set to an empty string during the Docker build so that all API calls from the SPA are relative (e.g. `/api/v1/...`), resolving against the same origin the SPA was served from.
- The Gateway serves the compiled SPA assets as static files.
- Any request path that does not match a static file or an API route returns `index.html`, enabling client-side routing (e.g. direct navigation to `/leagues/123` or `/callback`).
- API routes (`/api/*`) are handled by the API middleware and are never served by the static file fallback.
- The `web` service is removed from `docker-compose.yaml`. The `hub-gateway` service now serves the SPA at `http://localhost:5005` when the full local stack is running.
- `build/web/Dockerfile`, `build/web/Dockerfile.dockerignore`, and `build/web/nginx.conf` are removed as they are superseded by the gateway serving the SPA. The `build/web/makefile` targets (`web.build`, `web.lint`, `web.test`) are retained.
- Change detection in `zz-detect-changes.yml` is updated so that changes to `src/Worms.Hub.Web/**` or `build/web/**` trigger a gateway release, in the same way that changes to gateway source files do.

## Out of Scope

- Registering the production redirect URI (`https://worms.davideadie.dev/callback`) in Auth0 — this is a manual one-time step performed outside this slice.
- Any change to the gateway's `Cors:AllowedOrigins` production config — CORS is not needed between the SPA and API as they are same-origin.
- Any new Azure resources (no Azure Static Web Apps, no Cloudflare Worker, no Azure Front Door).
- Any change to the `deploy-main.yml` infrastructure workflow — no new Pulumi resources are introduced.
- Changes to the local Vite dev server workflow — developers can continue to run `npm run dev` directly against `http://localhost:5005` for hot-reload development.

## Acceptance Criteria

- Given the gateway Docker image is built, when it starts, a browser request to `https://worms.davideadie.dev/` returns the SPA landing page.
- Given a signed-in user navigates directly to `https://worms.davideadie.dev/leagues`, the server returns `index.html` and the SPA renders the league list page.
- Given a user navigates directly to `https://worms.davideadie.dev/callback`, the server returns `index.html` and the OIDC callback flow completes correctly.
- Given the SPA makes an API call (e.g. `GET /api/v1/leagues`), the request is handled by the Gateway API and returns JSON — it is not intercepted by the static file or fallback middleware.
- Given only web files change on a PR or merge to main, the gateway release job runs and a new gateway image is published.
- Given `docker compose up` is run locally, navigating to `http://localhost:5005` serves the SPA and API calls to `http://localhost:5005/api/v1/...` succeed.
- Given `docker-compose.yaml` is run, there is no separate `web` service — the SPA is served by `hub-gateway`.

## Open Questions

None.
