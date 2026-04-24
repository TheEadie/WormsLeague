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

JWT Bearer via `Microsoft.AspNetCore.Authentication.JwtBearer`. Config keys:

```
WORMS_AUTH__AUTHORITY
WORMS_AUTH__AUDIENCE
WORMS_AUTH__NAMECLAIM
WORMS_AUTH__PERMISSIONSCLAIM
```

In development (`ASPNETCORE_ENVIRONMENT=Development`), `MapControllers()` is called without the `.RequireAuthorization()` default override — but the `[Authorize]` attribute on the base controller still applies unless explicitly commented out.

## Announcers

`IAnnouncer` has two methods: `AnnounceGameStarting(hostName)` and `AnnounceGameComplete(winner)`. The only implementation is `SlackAnnouncer` which POSTs to a Slack incoming webhook configured as `WORMS_SLACKWEBHOOKURL`. In DEBUG builds the `<!here>` mention is stripped to avoid noisy notifications during development.

## Worker (queue consumer)

`CheckForMessagesService` is an `IHostedService` that polls the queue. `Processor` (in the Gateway project) handles the `replays-to-update` queue: it reads the log file, updates the DB, announces the winner, then deletes the message.

Note: there is a separate `Worms.Hub.Armageddon.Runner` project with its own `Processor` that handles `replays-to-process` — these are distinct pipelines.

## Service registration

`ServiceRegistration` in the Gateway project provides two extension methods:
- `AddGatewayServices()` — registers `IAnnouncer`, validators, HTTP client
- `AddWorkerServices()` — registers its `Processor`, plus pulls in Storage, Queue, Files, and Announcer services

## Configuration

All config is read via `IConfiguration`. Connection strings use the `ConnectionStrings:*` section (e.g. `ConnectionStrings:Storage`, `ConnectionStrings:Database`). Storage folder paths use `Storage:*` (e.g. `Storage:TempReplayFolder`, `Storage:CliFolder`, `Storage:SchemesFolder`, `Storage:GameFolder`).

## Telemetry

OpenTelemetry is configured via `AddOpenTelemetryWormsHub()` (extension in `Telemetry.cs`). Spans use `ActivityKind.Server` for HTTP handling and `ActivityKind.Consumer` for queue processing.
