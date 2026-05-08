# Review — Local-dev Integration

## Verdict

The implementation satisfies all acceptance criteria. Three files were added or modified exactly as planned, `make build` exits clean (0 warnings, 0 errors), `make web.lint` passes, and `docker compose build web` builds both stages successfully. The only finding is that the base images in the new Dockerfile use floating tags while the rest of the repo pins by digest — a suggestion for consistency, not a blocker.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| `docker compose up` starts all services including `web` without errors | MET | `docker compose build web` completes cleanly; service definition is syntactically valid and appended correctly in `docker-compose.yaml:106-111` |
| Browser reaches `http://localhost:3000` and placeholder loads | MET | nginx stage copies built SPA bundle to `/usr/share/nginx/html`; default nginx config serves on port 80; host port 3000 mapped via `ports: "3000:80"` |
| Existing services (`azure-storage`, `database`, `flyway-init`, `hub-gateway`, `hub-worker`, `hub-wa-runner`) behaviour unchanged | MET | `docker-compose.yaml` diff is purely additive — only 7 lines appended after `hub-wa-runner`; no existing service is touched |
| `make build` completes successfully | MET | `make build` exits 0; 0 warnings, 0 errors across gateway, wa-runner, cli, and web targets |

## Scope

Diff matches the plan's Files to Create / Modify table exactly:

| Planned path | Status |
|---|---|
| `build/web/Dockerfile` | Created — matches plan spec verbatim |
| `build/web/Dockerfile.dockerignore` | Created — matches plan spec verbatim |
| `docker-compose.yaml` | Modified — 7-line `web` service block appended |

The `learnings.md` confirms no deviations. The `.claude/specs/web-ui/plan.md` change (marking slice 03 `[x]`) is a workflow artefact and is ignored per review rules.

## Blockers

None.

## Suggestions

#### S1 — Pin base image digests

- **File:** `build/web/Dockerfile:2,12`
- **Issue:** `node:22-alpine` and `nginx:alpine` are floating tags. Both existing Dockerfiles in this repo (`build/docker/gateway/Dockerfile:2`, `build/docker/wa-runner/Dockerfile:2,35`) pin every base image with `@sha256:…`, which ensures reproducible builds and prevents silent upstream changes.
- **Fix:** Resolve current digests (`docker pull node:22-alpine --platform linux/amd64 && docker inspect … | jq '.[].RepoDigests'`) and append `@sha256:<digest>` to both `FROM` lines.
- **Decision:** — *(pending)*

## Nitpicks

None.

## Tests

No tests were added, and none are expected. This slice is pure Docker and Compose configuration; the only meaningful verification is runtime (`docker compose up`, browser check). The testing-strategy doc identifies integration tests as appropriate for "verifying behaviour at a deployment boundary (image starts, env vars are read correctly, services compose)" but the spec makes no requirement for automated coverage here, and that kind of test would duplicate the manual verification described in the plan. No coverage gap to flag.

## Recommended Actions

- **S1** — Accept — The pattern is already established by both existing Dockerfiles; pinning now avoids a future surprise when the upstream tag moves, and the cost is one `docker inspect` command.
