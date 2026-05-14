# Plan: Production Deployment

## Context

This slice integrates the SPA into the Gateway Docker image so that the Gateway serves the SPA as static files at `worms.davideadie.dev`. Earlier slices have: scaffolded the React/TypeScript SPA in `src/Worms.Hub.Web/` (slice 01); wired the web build job into CI with `zz-build-web.yml` (slice 02); added the `web` Docker service to `docker-compose.yaml` (slice 03); and configured CORS on the Gateway so browser requests from `http://localhost:3000` and `http://localhost:5173` succeed (slice 04). This slice replaces that separate `web` Docker service and separate nginx container with a single gateway image that builds the SPA internally and serves its static assets via ASP.NET Core static files middleware. It also updates change detection so that SPA file changes trigger a gateway release.

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `build/docker/gateway/Dockerfile` | Add a Node build stage before the .NET stage; copy SPA assets into `wwwroot/` inside the dotnet publish output |
| `build/docker/gateway/Dockerfile.dockerignore` | Allow `src/Worms.Hub.Web/` context files through; exclude `node_modules` |
| `src/Worms.Hub.Gateway/Program.cs` | Add `UseStaticFiles()` and `MapFallbackToFile("index.html")` after controller mapping |
| `src/Worms.Hub.Gateway/appsettings.Development.json` | Remove `http://localhost:3000` from `Cors:AllowedOrigins` |
| `docker-compose.yaml` | Remove the `web` service |
| `.github/workflows/zz-detect-changes.yml` | Add `src/Worms.Hub.Web/**` and `build/web/**` to the `gateway:` filter |

### Deleted files

| Path | Reason |
|---|---|
| `build/web/Dockerfile` | Superseded; the SPA is now built inside the gateway Dockerfile |
| `build/web/Dockerfile.dockerignore` | No longer needed without the separate web Docker build |
| `build/web/nginx.conf` | nginx is no longer used; ASP.NET Core serves the SPA |

---

## Implementation Details

### 1. Gateway Dockerfile — Node build stage

Add a new `node-build` stage before the existing `.NET build` stage. This stage installs dependencies and compiles the SPA with `VITE_GATEWAY_URL` set to an empty string, producing relative API URLs.

The full updated `build/docker/gateway/Dockerfile`:

```dockerfile
#### Web Build ####
FROM node:24-alpine AS node-build
WORKDIR /repo

COPY src/Worms.Hub.Web/package.json src/Worms.Hub.Web/package-lock.json src/Worms.Hub.Web/
RUN cd src/Worms.Hub.Web && npm ci

COPY src/Worms.Hub.Web/. src/Worms.Hub.Web/
RUN cd src/Worms.Hub.Web && VITE_GATEWAY_URL= npx vite build

#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:10.0.300@sha256:dc8430e6024d454edadad1e160e1973be3cabbb7125998ef190d9e5c6adf7dbb AS build
WORKDIR /app

COPY Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Hub.Queues/Worms.Hub.Queues.csproj ./src/Worms.Hub.Queues/Worms.Hub.Queues.csproj
COPY src/Worms.Hub.Storage/Worms.Hub.Storage.csproj ./src/Worms.Hub.Storage/Worms.Hub.Storage.csproj
COPY src/Worms.Armageddon.Files/Worms.Armageddon.Files.csproj ./src/Worms.Armageddon.Files/Worms.Armageddon.Files.csproj
COPY src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj ./src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj
RUN dotnet restore src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj

COPY src/Worms.Hub.Queues ./src/Worms.Hub.Queues
COPY src/Worms.Hub.Storage ./src/Worms.Hub.Storage
COPY src/Worms.Armageddon.Files ./src/Worms.Armageddon.Files
COPY src/Worms.Hub.Gateway ./src/Worms.Hub.Gateway
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj \
    -c Release \
    -o out \
    --no-restore \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

COPY --from=node-build /repo/.artifacts/web/ ./out/wwwroot/

#### Test ####
FROM build AS test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM mcr.microsoft.com/dotnet/aspnet:10.0.8@sha256:9b5222b0ff8e9eb991a7c1a64b25f0f771d21ccc05dfa1c834f5668ffd9cd73f
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
```

Key points:
- `VITE_GATEWAY_URL=` (empty value) makes all API calls relative, resolving against the same origin as the page.
- The `vite.config.ts` `outDir` is `../../.artifacts/web/` relative to `src/Worms.Hub.Web/`, which resolves to `/repo/.artifacts/web/` given the `WORKDIR /repo` above. The `COPY --from=node-build` pulls from that path.
- The `COPY --from=node-build` line runs after `dotnet publish` so the SPA assets land in `out/wwwroot/` — the directory that `UseStaticFiles()` will serve from (ASP.NET Core's default static-file root).
- The `test` stage inherits from `build` (which already has the SPA in `out/wwwroot/`), so no changes are needed there.
- The node image uses the same `node:24-alpine` base as the existing `build/web/Dockerfile`; keep the pinned digest from that file: `node:24-alpine@sha256:d1b3b4da11eefd5941e7f0b9cf17783fc99d9c6fc34884a665f40a06dbdfc94f`.

### 2. Gateway Dockerfile.dockerignore — allow web source

The existing `build/docker/gateway/Dockerfile.dockerignore` already allows `!/src`, which covers `src/Worms.Hub.Web/`. However, `node_modules` (which may exist locally after development) must be excluded so it is not sent to the Docker build context. Add the exclusion:

```
# Ignore everything
**

# Except for these files
!/src
!.editorconfig
!Directory.Build.props

**/bin
**/obj
src/Worms.Hub.Web/node_modules
```

No other changes are required — the `!/src` rule already allows `src/Worms.Hub.Web/` through.

### 3. Gateway Program.cs — static files and SPA fallback

ASP.NET Core serves static files from `wwwroot/` under `ContentRootPath` by default. The SPA assets will be copied there by the Dockerfile (see section 1). No additional NuGet packages are required — both `UseStaticFiles()` and `MapFallbackToFile()` are part of `Microsoft.AspNetCore.Builder` in the `Microsoft.AspNetCore.App` framework reference.

The middleware must be added inside the `if (runGateway)` block in `Program.cs`, after `MapControllers()`. The correct ordering ensures:

1. API requests (`/api/*`) are handled by the controller mapping and never reach the static-file middleware.
2. Known static files (JS, CSS, etc.) are served by `UseStaticFiles()` before the fallback.
3. Any other request (e.g. `/leagues/123`, `/callback`, `/`) falls back to `index.html`.

Replace the existing gateway `app` configuration section (the `if (runGateway)` block after `builder.Build()`):

```csharp
if (runGateway)
{
    _ = app.UseHttpsRedirection();
    _ = app.UseRouting();
    _ = app.UseCors(corsPolicyName);
    _ = app.UseStaticFiles();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        _ = app.UseDeveloperExceptionPage();
        _ = app.MapControllers();
    }
    else
    {
        _ = app.MapControllers();
    }

    _ = app.MapFallbackToFile("index.html");
    _ = app.UseRequestLogging();
}
```

`UseStaticFiles()` is placed after `UseCors()` (browsers send preflight OPTIONS to static resources too) and before authentication (static assets are publicly served without a token). `MapFallbackToFile("index.html")` is placed after `MapControllers()` so it only fires for paths that no controller matched.

Note: `UseStaticFiles()` requires no arguments when assets live in `wwwroot/` relative to the content root — this is the ASP.NET Core convention.

### 4. appsettings.Development.json — remove old web service origin

With the `web` Docker service removed, `http://localhost:3000` (the port that service exposed) is no longer a valid origin. The Vite dev server (`http://localhost:5173`) remains valid for developers running `npm run dev`. Remove `http://localhost:3000`:

```json
{
  "Logging": { ... },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  },
  ...
}
```

### 5. docker-compose.yaml — remove the web service

Remove the `web` service block entirely from `docker-compose.yaml`. The `hub-gateway` service already maps port `5005:8080` and will now serve the SPA from that port. No other changes to `docker-compose.yaml` are needed.

The block to remove:

```yaml
    web:
        build:
            dockerfile: build/web/Dockerfile
            context: .
        ports:
            - "3000:80"
```

### 6. zz-detect-changes.yml — wire web changes to gateway release

The `gateway:` filter in `zz-detect-changes.yml` currently watches gateway source files and docker build files. Add `src/Worms.Hub.Web/**` and `build/web/**` to this filter so that changes to the SPA source or its build configuration also trigger a gateway release:

```yaml
      gateway:
        - 'src/Worms.Armageddon.Files/**'
        - 'src/Worms.Hub.Gateway/**'
        - 'src/Worms.Hub.Queues/**'
        - 'src/Worms.Hub.Storage/**'
        - 'src/Worms.Hub.Web/**'
        - 'build/docker/gateway/**'
        - 'build/web/**'
```

The `web-build` output and the `web:` filter are left unchanged — they are retained for any future web-specific CI step.

### 7. Delete build/web Docker files

Delete the following three files. They are no longer needed because the gateway Dockerfile handles the SPA build and ASP.NET Core handles static file serving:

- `build/web/Dockerfile`
- `build/web/Dockerfile.dockerignore`
- `build/web/nginx.conf`

The `build/web/makefile` is **retained** — it provides `web.build`, `web.lint`, and `web.test` targets used by CI and local development.

---

## Verification

1. Run `make gateway.build` locally (requires Docker Buildx). The build should complete with no errors. Both the Node and .NET stages should succeed.
2. Run `docker compose up hub-gateway` and navigate to `http://localhost:5005` in a browser. The SPA landing page should load.
3. Navigate directly to `http://localhost:5005/leagues` in a browser (without signing in). The server should return `index.html` and the SPA should render (redirecting to sign-in).
4. Confirm an API call works: `curl http://localhost:5005/api/v1/leagues` should return a JSON response (possibly 401), not the `index.html` file.
5. Confirm `http://localhost:3000` is no longer served by any service (`docker compose ps` should show no container on port 3000).
6. In `zz-detect-changes.yml`, verify that a PR touching only `src/Worms.Hub.Web/src/App.tsx` produces `gateway-release: true` in the detect-changes job output.
7. Run `make gateway.package` (requires Docker Buildx + Docker Hub credentials) to confirm the full multi-platform image builds without error.
