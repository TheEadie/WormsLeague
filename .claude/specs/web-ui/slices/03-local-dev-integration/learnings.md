# Learnings: Local-dev Integration (Slice 03)

## Implementation Notes

### Everything went exactly as planned

The plan was complete and accurate. All three files were created/modified exactly as described:

- `build/web/Dockerfile` — multi-stage build worked first time; the WORKDIR reasoning was correct: `vite build` resolves `outDir: '../../.artifacts/web/'` to `/repo/.artifacts/web/` and nginx picks it up cleanly.
- `build/web/Dockerfile.dockerignore` — naming convention matches `build/docker/gateway/Dockerfile.dockerignore` as described.
- `docker-compose.yaml` — the `web` service appended after `hub-wa-runner` with no `depends_on` required.

`docker compose build web` completed without errors. Both stages ran successfully (npm ci, tsc -b, vite build, nginx copy). The deprecation warnings from `eslint@8.57.1` during `npm ci` are pre-existing cosmetic noise (noted in slice 02 learnings) and do not affect the build.

### Plan note about future nginx.conf is accurate

The plan correctly calls out that a `try_files $uri $uri/ /index.html;` nginx config will be needed when client-side routing is introduced. The default nginx config is sufficient for the current placeholder page.
