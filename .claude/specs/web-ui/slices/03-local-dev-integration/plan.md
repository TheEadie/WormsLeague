# Plan: Local-dev Integration

## Context

This slice wires the React SPA into the existing `docker compose up` stack. The SPA was scaffolded in slice 01 and already builds to `.artifacts/web/` via `make web.build`. This slice adds a Dockerfile that builds and serves the static bundle via nginx, and adds a corresponding service to `docker-compose.yaml` so `docker compose up` starts it alongside the existing hub services.

Production make targets (docker bake, Docker Hub publish) are explicitly out of scope — those are deferred to the production deployment slice.

Builds on:
- Slice 01: `src/Worms.Hub.Web/` exists, `build/web/makefile` exists, `vite.config.ts` has `outDir: '../../.artifacts/web/'`
- No changes are needed to `vite.config.ts` or the web source code.

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `build/web/Dockerfile` | Multi-stage: Node 22 Alpine builds the SPA bundle; nginx Alpine serves it |
| `build/web/Dockerfile.dockerignore` | Restricts Docker build context to `src/Worms.Hub.Web/` only |

### Modified files

| Path | Change |
|---|---|
| `docker-compose.yaml` | Add `web` service: builds from `build/web/Dockerfile` with context `.`, exposes port `3000:80` |

---

## Implementation Details

### 1. Dockerfile placement: `build/web/Dockerfile` (not `build/docker/web/`)

The root `build/docker/makefile` auto-discovers Dockerfiles with:
```makefile
DOCKER_COMPONENTS := $(shell find build/docker/* -maxdepth 1 -mindepth 1 -name Dockerfile | cut -f3,3 -d/)
```
Placing the Dockerfile at `build/web/Dockerfile` keeps it invisible to that discovery and avoids the need for `config.mk` / `docker-bake.hcl` stubs. `make build` continues to work unchanged — `build/web/makefile` only contains `web.build` and `web.lint`, and is not extended with docker bake targets.

### 2. Dockerfile: `build/web/Dockerfile`

```dockerfile
#### Build ####
FROM node:22-alpine AS build
WORKDIR /repo/src/Worms.Hub.Web

COPY src/Worms.Hub.Web/package.json src/Worms.Hub.Web/package-lock.json ./
RUN npm ci

COPY src/Worms.Hub.Web/. .
RUN npm run build

#### Runtime ####
FROM nginx:alpine
COPY --from=build /repo/.artifacts/web /usr/share/nginx/html
```

**WORKDIR reasoning**: Setting WORKDIR to `/repo/src/Worms.Hub.Web` mirrors the actual source tree so `vite.config.ts`'s `outDir: '../../.artifacts/web/'` resolves to `/repo/.artifacts/web/` without any changes to the config. No `--outDir` override is needed.

**Node version**: `node:22-alpine` matches the `node-version: 22` used in CI (`actions/setup-node`).

**`npm run build`**: This is `tsc -b && vite build` (from `package.json`). Both `tsconfig.json` and `vite.config.ts` are present in the WORKDIR after `COPY src/Worms.Hub.Web/. .`.

**nginx**: The default nginx Alpine image listens on port 80 and serves from `/usr/share/nginx/html`. No custom nginx config is needed for this slice — the placeholder is a single page with no client-side routing. When routing is introduced in a later slice, a custom `nginx.conf` with `try_files $uri $uri/ /index.html;` will be needed.

### 3. Dockerignore: `build/web/Dockerfile.dockerignore`

Follows the naming convention used by the other Dockerfiles (e.g., `build/docker/gateway/Dockerfile.dockerignore`):

```
# Ignore everything
**

# Except for these files
!/src/Worms.Hub.Web

# Exclude node_modules (large, and must not be used inside the container build)
src/Worms.Hub.Web/node_modules
```

### 4. docker-compose.yaml: add `web` service

Append after the `hub-wa-runner` service, following the existing indentation (4-space):

```yaml
    web:
        build:
            dockerfile: build/web/Dockerfile
            context: .
        ports:
            - "3000:80"
```

No `depends_on` is required — the web service serves purely static files and has no runtime dependency on the gateway, database, or storage at startup.

---

## Verification

1. `make build` — completes successfully; no new docker bake targets are introduced that could break.
2. `docker compose build web` — both build stages complete without errors.
3. `docker compose up` — all services (including `web`) reach running state without errors.
4. Open `http://localhost:3000` in a browser — the "Worms League" placeholder page loads.
5. Confirm existing services are unaffected: gateway at `http://localhost:5005`, storage on ports `10000–10002`, database on port `5432`.
