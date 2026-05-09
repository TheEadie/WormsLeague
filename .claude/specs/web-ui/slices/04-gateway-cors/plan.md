# Plan: Gateway CORS

## Context

This slice adds CORS support to the Hub Gateway so that the React SPA (delivered in slices 01–03)
can make browser requests to the existing JSON API. Slices 01–03 put the SPA in place and wired it
into local dev via `docker compose`; this slice is the first change to a .NET project in the epic.

No new packages are required — CORS middleware ships with `Microsoft.AspNetCore`. The change is
limited to three existing files in `Worms.Hub.Gateway`: `Program.cs` and the two appsettings files.
No controllers, DTOs, auth configuration, or other Gateway logic are touched.

---

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Gateway/Program.cs` | Add `AddCors` in the service-registration block and `UseCors` in the middleware pipeline |
| `src/Worms.Hub.Gateway/appsettings.json` | Add an empty `Cors.AllowedOrigins` array — no hardcoded origins |
| `src/Worms.Hub.Gateway/appsettings.Development.json` | Add the two local-dev origins to `Cors.AllowedOrigins` |

---

## Implementation Details

### 1. Configuration binding

The config key path is `Cors:AllowedOrigins`. The `AddEnvironmentVariables("WORMS_")` call in
`Program.cs` already strips the `WORMS_` prefix and converts `__` to `:`, so the env var
`WORMS_CORS__ALLOWEDORIGINS__0`, `WORMS_CORS__ALLOWEDORIGINS__1`, etc. maps correctly to the
`Cors:AllowedOrigins` array section. (For production, the deployment slice sets env vars using
this indexed notation.)

Read the value with:

```csharp
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
```

This works with both a JSON array in `appsettings*.json` and with indexed environment variables
(`__0`, `__1`). When the section is absent or empty, the result is an empty array — no CORS
headers are emitted.

### 2. appsettings.json — base config (no hardcoded origins)

Add an empty array under `Cors`. The file currently has no `Cors` section. Insert after the
`AllowedHosts` entry:

```json
{
  "Logging": { ... },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": []
  },
  "Auth": { ... },
  "ConnectionStrings": { ... }
}
```

An empty array means `.WithOrigins()` is called with no origins, so the policy matches nothing
and no `Access-Control-Allow-Origin` header is emitted. This satisfies the acceptance criterion
that the base config emits no CORS headers.

### 3. appsettings.Development.json — local-dev origins

Add the two Vite/docker-compose origins. The file currently has only `Logging` and `Storage`
sections. Insert after `Logging`:

```json
{
  "Logging": { ... },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  },
  "Storage": { ... }
}
```

`http://localhost:3000` is the nginx container exposed by `docker compose up` (the `web` service,
mapped to host port 3000). `http://localhost:5173` is the default Vite dev server port for
running `npm run dev` directly on the host.

The `hub-gateway` container already sets `ASPNETCORE_ENVIRONMENT=Development` in
`docker-compose.yaml`, so this file is picked up automatically — no change to `docker-compose.yaml`
is needed.

### 4. Program.cs — service registration

Inside the `if (runGateway)` block, read the allowed origins from configuration and register a
named CORS policy called `"WormsWebUi"`. Add this **before** the `AddControllers` call (ordering
within the builder phase does not matter functionally, but placing it first groups infrastructure
middleware together):

```csharp
const string corsPolicyName = "WormsWebUi";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
_ = builder.Services.AddCors(options =>
    options.AddPolicy(corsPolicyName, policy =>
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Authorization", "Content-Type")));
```

`AllowCredentials()` is explicitly omitted — the UI authenticates via JWT bearer in the
`Authorization` header, not cookies.

### 5. Program.cs — middleware pipeline

Inside the `if (runGateway)` block that configures the `WebApplication`, insert `app.UseCors()`
**after** `UseRouting` and **before** `UseAuthentication`. The correct ASP.NET Core middleware
order for CORS is:

```
UseHttpsRedirection
UseRouting
UseCors          ← insert here
UseAuthentication
UseAuthorization
MapControllers / UseRequestLogging
```

The current code in `Program.cs` does not call `UseRouting()` explicitly (it is implicit in
`MapControllers()`). With implicit routing, CORS middleware must still appear before
`UseAuthentication`. The safe position is immediately before `UseAuthentication`:

```csharp
_ = app.UseHttpsRedirection();
_ = app.UseRouting();           // add explicit UseRouting call
_ = app.UseCors(corsPolicyName);
_ = app.UseAuthentication();
_ = app.UseAuthorization();
```

Note: `corsPolicyName` must be in scope at the point `app` is configured. Because `var
corsPolicyName` is declared before `builder.Build()`, it is naturally in scope for both the
`builder.Services` and `app` configuration sections.

The `UseCors()` call must appear in **both** the `if (app.Environment.IsDevelopment())` branch
and the `else` branch — or, more cleanly, before the `if`/`else` split entirely, since CORS
applies in all environments:

```csharp
_ = app.UseHttpsRedirection();
_ = app.UseRouting();
_ = app.UseCors(corsPolicyName);
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

_ = app.UseRequestLogging();
```

### 6. Formatting constraints

The file uses 4-space indentation, Allman braces, and 120-character line length (enforced by
`.editorconfig`). The `AddCors` lambda chain should be formatted to stay within 120 characters.
The `_ =` discard pattern is used consistently in this file for `IApplicationBuilder` return
values — continue that pattern.

### 7. No test changes

There is no dedicated unit-test project for `Worms.Hub.Gateway`. The testing strategy notes that
Gateway behaviour is exercised via integration tests and smoke testing. CORS behaviour requires a
running HTTP stack (the behaviour is in middleware, not in application logic), so it is not
amenable to pure unit testing. Verification is via `curl` against the local dev stack (see
Verification section below).

---

## Verification

1. Build the Gateway to confirm no compiler errors:
   ```
   make gateway.build
   ```

2. Start the local dev stack:
   ```
   docker compose up
   ```

3. Check a simple CORS preflight from the `http://localhost:3000` origin — confirm the gateway
   returns a 204 with `Access-Control-Allow-Origin: http://localhost:3000` and
   `Access-Control-Allow-Headers` includes `Authorization`:
   ```
   curl -s -o /dev/null -D - \
     -X OPTIONS http://localhost:5005/api/v1/leagues \
     -H "Origin: http://localhost:3000" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: Authorization"
   ```
   Expected response headers:
   - `access-control-allow-origin: http://localhost:3000`
   - `access-control-allow-headers: Authorization`
   - HTTP status 204

4. Repeat the same request with `Origin: http://localhost:5173` and confirm the same headers are
   returned.

5. Send the same preflight with an unlisted origin (e.g., `http://localhost:9999`) and confirm
   there is no `access-control-allow-origin` header in the response.

6. Send an actual `GET` request with a valid bearer token and `Origin: http://localhost:3000`
   and confirm the API responds normally (auth behaviour unchanged):
   ```
   curl -s -o /dev/null -D - \
     -H "Authorization: Bearer <token>" \
     -H "Origin: http://localhost:3000" \
     http://localhost:5005/api/v1/leagues
   ```
   Expected: HTTP 200 (or 401/403 if the token is invalid — the point is CORS headers are
   present and the request is not rejected by CORS middleware).
