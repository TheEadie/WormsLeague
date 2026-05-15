# Hub Gateway Component

Project: `Worms.Hub.Gateway`

## Operating modes

The single binary can run in different modes controlled by `WORMS_`-prefixed environment variables. `Program.cs` reads these at startup:

| Env var | Value | Effect |
|---|---|---|
| `WORMS_HUB_DISTRIBUTED` | `true` | Enable distributed mode (gateway and worker run separately) |
| `WORMS_HUB_GATEWAY` | `true` | Run the HTTP API (requires `HUB_DISTRIBUTED=true`) |
| `WORMS_HUB_WORKER` | `true` | Run the queue consumer (requires `HUB_DISTRIBUTED=true`) |
| `WORMS_BATCH` | `true` | Process one replay message then exit |

Without `HUB_DISTRIBUTED`, both gateway and worker run in the same process (monolith).

## API controllers

All controllers inherit `V1ApiController` which sets:
- `[ApiVersion("1.0")]`
- Route: `api/v{version:apiVersion}/[controller]`
- `[Authorize(Roles = "access")]`

Controllers are declared `internal sealed`. `InternalControllerProvider` overrides `ControllerFeatureProvider.IsController()` to detect them by `[ApiController]` attribute rather than public visibility.

Versioning uses `Microsoft.AspNetCore.Mvc.Versioning` (`AddApiVersioning()`).

## Auth

JWT Bearer (Auth0-style) via `Microsoft.AspNetCore.Authentication.JwtBearer`. The CLI authenticates against the same authority via device flow. Config keys:

```
WORMS_AUTH__AUTHORITY
WORMS_AUTH__AUDIENCE
WORMS_AUTH__NAMECLAIM
WORMS_AUTH__PERMISSIONSCLAIM
```

In development (`ASPNETCORE_ENVIRONMENT=Development`), `MapControllers()` is called without the `.RequireAuthorization()` default override — but the `[Authorize]` attribute on the base controller still applies unless explicitly commented out.

## Announcers

`IAnnouncer` has two methods: `AnnounceGameStarting(hostName)` and `AnnounceGameComplete(winner, placements?)`. The only implementation is `SlackAnnouncer` which POSTs to a Slack incoming webhook configured as `WORMS_SLACKWEBHOOKURL`. In DEBUG builds the `<!here>` mention is stripped to avoid noisy notifications during development.

## Worker (queue consumer)

`CheckForMessagesService` is an `IHostedService` that polls the queue. `Processor` (in `Worms.Hub.Gateway/Worker/`) handles the `replays-to-update` queue: it reads the log file, updates the DB, announces the winner, then deletes the message.

Note: there is a separate `Worms.Hub.Armageddon.Runner` project with its own `Processor` that handles `replays-to-process` — these are distinct pipelines.

## Replay processing flow

End-to-end flow for a replay upload:

1. CLI uploads a `.WAgame` file to the gateway (`ReplaysController`)
2. Gateway stores the file and enqueues a `ReplayToProcessMessage` on `replays-to-process`
3. WA Runner dequeues it, runs the game to extract the log and GIFs, then enqueues a `ReplayToUpdateMessage` on `replays-to-update`
4. Gateway worker (`CheckForMessagesService` → `Worker/Processor`) dequeues, reads the log, updates the DB, announces the winner to Slack

## Service registration

`ServiceRegistration` in the Gateway project provides two extension methods:
- `AddGatewayServices()` — registers `IAnnouncer`, validators, HTTP client
- `AddWorkerServices()` — registers its `Processor`, plus pulls in Storage, Queue, Files, and Announcer services

Services used by both the gateway and the worker (e.g. `AddWormsArmageddonFilesServices()`, which registers `IReplayTextReader`) must be called inside **each** component's own `Add*Services()` method, not placed unconditionally in `Program.cs`. Because `AddGatewayServices()` and `AddWorkerServices()` are both called in monolith mode, any method registered from multiple components must use `TryAddScoped` / `TryAddSingleton` / `TryAddEnumerable` throughout so repeated calls in monolith mode are safe (no double-registered parsers or handlers).

## Feature flags

Controllers must depend on `IFeatureFlags`, not on `DatabaseSchemaVersion` or any other concrete source directly. `GatewayFeatureFlags` aggregates all feature-gate sources (schema version, environment variables, etc.) and is the single place where feature decisions are made. Any new schema-version or feature-gate check must be added to `GatewayFeatureFlags` rather than injected into a controller.

## Middleware ordering

Inside the `if (runGateway)` block in `Program.cs`, `UseRequestLogging()` must be the **first** middleware call, before `UseStaticFiles()`, `MapControllers()`, and `MapFallbackToFile()`. Middleware placed after endpoint dispatch misses requests short-circuited by static-file serving and fallback routing.

## Deployment safety

Any new endpoint backed by a DB migration must address independent gateway/DB rollout risk. If the gateway can be deployed before the migration runs, it will crash rather than degrade gracefully. Mitigate with one of:

- **Schema-version gate:** check `DatabaseSchemaVersion` (via `IFeatureFlags`) before querying the new table and return a suitable fallback or 503.
- **Feature flag:** hide the endpoint until the migration has been confirmed applied.

This decision must be made during spec — not discovered after deployment.

## DTOs

When adding an endpoint that returns a domain type already served by an existing endpoint, reuse and extend the existing DTO rather than creating a parallel type. Derived `bool` fields (e.g. `Processed`) must not be introduced when the domain model already carries an equivalent discriminant as a string (e.g. `Status`) — the TypeScript interface on the frontend must match the actual serialised JSON shape.

When a new non-nullable-in-practice column is added to an existing table, search for all controller actions and worker methods that call `repository.Create()` or `repository.Update()` for the affected type and confirm each one sets the new field. Do not rely on the repository implementation alone to surface missing write-path callers.

## Configuration

All config is read via `IConfiguration`. Connection strings use the `ConnectionStrings:*` section (e.g. `ConnectionStrings:Storage`, `ConnectionStrings:Database`). Storage folder paths use `Storage:*` (e.g. `Storage:TempReplayFolder`, `Storage:CliFolder`, `Storage:SchemesFolder`, `Storage:GameFolder`).

## Telemetry

OpenTelemetry is configured via `AddOpenTelemetryWormsHub()` (extension in `Telemetry.cs`). Spans use `ActivityKind.Server` for HTTP handling and `ActivityKind.Consumer` for queue processing.
