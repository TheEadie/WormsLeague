# Review — Production Deployment

## Verdict

The implementation satisfies every acceptance criterion in the spec. All six planned file modifications and three deletions are present and match the plan exactly. The .NET build exits clean with zero warnings, and `make web.lint` passes. There are no blockers. One suggestion is raised about middleware ordering (the `UseRequestLogging()` placement is harmless today but will silently miss logging static-file and fallback requests), and one nitpick about the `node_modules` exclusion pattern placement in the dockerignore file.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Browser request to `https://worms.davideadie.dev/` returns the SPA landing page | MET | `build/docker/gateway/Dockerfile`: node-build stage compiles SPA assets into `/repo/.artifacts/web/`; `COPY --from=node-build` places them in `out/wwwroot/`; `Program.cs:66` calls `UseStaticFiles()` which serves `wwwroot/` by default |
| Direct navigation to `/leagues` returns `index.html` and the SPA renders | MET | `Program.cs:80` calls `MapFallbackToFile("index.html")` after `MapControllers()`, so unmatched paths fall through to the HTML document |
| Direct navigation to `/callback` returns `index.html` and OIDC callback completes | MET | Same `MapFallbackToFile` fallback; the OIDC callback is handled client-side by the SPA once `index.html` loads |
| API calls (`/api/*`) are handled by the Gateway and not intercepted by the fallback | MET | `MapControllers()` is called before `MapFallbackToFile()`; endpoint routing dispatches `/api/*` paths to controllers first |
| Web file changes trigger the gateway release job | MET | `zz-detect-changes.yml:45-47` adds `src/Worms.Hub.Web/**` and `build/web/**` to the `gateway:` filter |
| `docker compose up` serves the SPA at `http://localhost:5005` | MET | `docker-compose.yaml`: `hub-gateway` service maps `5005:8080` and the `web` service has been removed; the gateway image now includes the SPA |
| No separate `web` service in `docker-compose.yaml` | MET | `docker-compose.yaml`: the `web` service block has been removed |

## Scope

The diff matches the plan's Files to Create / Modify table exactly:

- `build/docker/gateway/Dockerfile` — modified (node-build stage added)
- `build/docker/gateway/Dockerfile.dockerignore` — modified (`node_modules` exclusion added)
- `src/Worms.Hub.Gateway/Program.cs` — modified (`UseStaticFiles()` and `MapFallbackToFile` added)
- `src/Worms.Hub.Gateway/appsettings.Development.json` — modified (`http://localhost:3000` removed)
- `docker-compose.yaml` — modified (`web` service removed)
- `.github/workflows/zz-detect-changes.yml` — modified (`src/Worms.Hub.Web/**` and `build/web/**` added to `gateway:` filter)
- `build/web/Dockerfile`, `build/web/Dockerfile.dockerignore`, `build/web/nginx.conf` — deleted

No files outside the plan were changed. `learnings.md` reports no deviations.

## Blockers

None.

## Suggestions

#### S1 — `UseRequestLogging()` placed outside the middleware pipeline

- **File:** `src/Worms.Hub.Gateway/Program.cs:81`
- **Issue:** `UseRequestLogging()` is called after `MapFallbackToFile()` in the builder sequence. In ASP.NET Core's endpoint routing model, `Use*` middleware placed after `Map*` registrations runs after endpoint dispatch, meaning requests served by `UseStaticFiles()` (which short-circuits before endpoint routing) and the fallback handler will not be logged.
- **Fix:** Move `UseRequestLogging()` to the top of the `if (runGateway)` block, immediately after `UseHttpsRedirection()`, so it wraps the full request pipeline. This matches the common pattern: logging middleware should be first in so it sees every request and every response code.
- **Decision:** Accept

## Nitpicks

#### N1 — `node_modules` exclusion at end of dockerignore rather than with other exclusions

- **File:** `build/docker/gateway/Dockerfile.dockerignore:11`
- **Issue:** The `**/bin` and `**/obj` exclusions are grouped together; `src/Worms.Hub.Web/node_modules` is added as a trailing line after them, which is slightly inconsistent with their grouping.
- **Fix:** Place `src/Worms.Hub.Web/node_modules` on the line immediately after `**/obj` so all exclusion rules are grouped together.
- **Decision:** Accept

## Tests

No tests were added or modified in this slice. This is appropriate: the changes are infrastructure/configuration — Dockerfile stages, middleware wiring, CI filter rules, and a docker-compose change. None of these have a meaningful unit test surface. The plan's verification steps are all manual/runtime checks, consistent with the testing strategy's guidance that deployment-boundary behaviour belongs in integration tests rather than unit tests. No coverage gap is introduced.

## Recommended Actions

- **S1** — Accept — The fix is a one-line move that ensures all request paths (static files and the SPA fallback) are covered by request logging, with no functional risk.
- **N1** — Accept — A one-line reorder that makes the dockerignore file easier to read at a glance; zero risk.
