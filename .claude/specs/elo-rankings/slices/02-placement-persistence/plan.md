# Plan: Placement Persistence

## Context

This slice extends Hub Storage and the Gateway Worker to persist per-team finish positions atomically alongside each replay update. It builds on the **Parser placement extraction** slice (already merged), which added `IReadOnlyCollection<Placement> Placements` to `ReplayResource` and the `Placement(Team Team, int Position)` record in `Worms.Armageddon.Files`. The `Processor` in `Worms.Hub.Gateway.Worker` already calls `replayTextReader.GetModel(replayLog)` and uses the result to populate `Winner` and `Teams`; this slice adds the mapping of `replayModel.Placements` to the new `Replay.Placements` field and ensures the V05 repository writes those rows to Postgres in the same transaction as the replay `UPDATE`.

A new Flyway migration (`V0.5`) creates `replay_placements`. The DI factory in `ServiceRegistration` is extended to select `ReplaysRepositoryV05` at schema ≥ V0.5. A startup `IHostedService` (`PlacementsBackfillService`) backfills all previously-processed replays when the schema is ready and the table is empty. `IFeatureFlags` gains `IsPlacementsEnabledAsync()` for gating the backfill.

**Scope decision — list endpoint asymmetry:** `GetAll()` and `GetByLeagueId()` on `ReplaysRepositoryV05` both return `Placements`. The `ReplaysController` list endpoint and `LeaguesController` replay endpoints both call `ReplayDto.FromDomain` / `ReplayDetailDto.FromDomain`, neither of which maps `Placements`. Those DTOs are left unchanged in this slice — placements are stored but not yet surfaced in any API response (that belongs to the "Placement display" slice).

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/database/migrations/V0.5__AddReplayPlacements.sql` | Flyway migration creating `replay_placements` table |
| `src/Worms.Hub.Storage/Domain/ReplayPlacement.cs` | New `ReplayPlacement` domain record |
| `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs` | V0.5 repository — transactional write + populated reads |
| `src/Worms.Hub.Gateway/Worker/PlacementsBackfillService.cs` | Startup `IHostedService` that backfills placement rows |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Storage/Domain/Replay.cs` | Add `IReadOnlyList<ReplayPlacement>? Placements` as final positional parameter |
| `src/Worms.Hub.Storage/Database/ReplaysRepositoryV04.cs` | Return `null` for `Placements` on all reads; ignore `Placements` on `Update()` — no SQL changes needed |
| `src/Worms.Hub.Storage/ServiceRegistration.cs` | Add V0.5 boundary: select `ReplaysRepositoryV05` when schema ≥ V0.5 |
| `src/Worms.Hub.Gateway/FeatureFlags/IFeatureFlags.cs` | Add `Task<bool> IsPlacementsEnabledAsync()` |
| `src/Worms.Hub.Gateway/FeatureFlags/FeatureFlags.cs` | Implement `IsPlacementsEnabledAsync()` — schema ≥ V0.5 check |
| `src/Worms.Hub.Gateway/ServiceRegistration.cs` | Register `IFeatureFlags` with `TryAddScoped` in `AddWorkerServices()` so the backfill service can depend on it in distributed (worker-only) mode |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Map `replayModel.Placements` → `IReadOnlyList<ReplayPlacement>` on the `Replay` passed to `replayRepository.Update()` |
| `src/Worms.Hub.Gateway/API/Controllers/ReplaysController.cs` | Fix positional `new Replay(...)` construction — add `null` for the new `Placements` parameter |
| `src/Worms.Hub.Gateway/Program.cs` | Register `PlacementsBackfillService` as a hosted service inside the `runWorker` block |

---

## Implementation Details

### 1. Database migration — `V0.5__AddReplayPlacements.sql`

File path: `src/database/migrations/V0.5__AddReplayPlacements.sql`

```sql
CREATE TABLE IF NOT EXISTS public.replay_placements (
    replay_id   integer NOT NULL REFERENCES public.replays (id),
    machine     text    NOT NULL,
    team_name   text    NOT NULL,
    position    integer NOT NULL,
    PRIMARY KEY (replay_id, machine, team_name)
);
```

`replay_id` is an `integer` FK to `replays.id` (Postgres auto-increment, stored as `int` in the DB). `machine` and `team_name` are plain `text`. `position` is 1-based with ties allowed (two rows can share the same value).

### 2. New domain record — `ReplayPlacement`

File: `src/Worms.Hub.Storage/Domain/ReplayPlacement.cs`

```csharp
using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record ReplayPlacement(string Machine, string TeamName, int Position);
```

`internal sealed` would suffice within the assembly, but because `Replay` (which carries `Placements`) is `[PublicAPI]` and consumed by the Gateway assembly, `ReplayPlacement` must also be `public`.

### 3. Extend `Replay` domain record

Add `IReadOnlyList<ReplayPlacement>? Placements` as the final positional parameter:

```csharp
[PublicAPI]
public sealed record Replay(
    string Id,
    string Name,
    string Status,
    string Filename,
    string? FullLog,
    string? LeagueId,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams,
    IReadOnlyList<ReplayPlacement>? Placements);
```

`null` means placements are unavailable (schema < V0.5 or not yet computed). An empty list means the replay was processed but yielded no placement data.

**Call sites that construct `Replay` positionally** and must be updated:
- `ReplaysController.cs` line 35: `repository.Create(new Replay("0", ..., null, null, null))` — append a final `null` for `Placements`.
- `ReplayDb.ToDomain()` in `ReplayDb.cs` — append `null` (V04 reads never populate placements).

`with` expressions (used in `Processor.cs` and repository implementations) are not affected by adding a new positional parameter — `with { ... }` leaves unmentioned properties at their current value.

### 4. `ReplayDb` mapping type — add `Placements = null`

`ReplayDb.ToDomain()` currently returns:
```csharp
new(Id, Name, Status, Filename, FullLog, LeagueId, Date, Winner, Teams);
```

Extend to:
```csharp
new(Id, Name, Status, Filename, FullLog, LeagueId, Date, Winner, Teams, null);
```

No new `ReplayPlacementDb` type is needed in `ReplayDb.cs` — placements are queried separately in `ReplaysRepositoryV05` and joined in C#, not via Dapper multi-mapping. A separate `ReplayPlacementDb` type lives inside `ReplaysRepositoryV05.cs` (see §6).

### 5. `ReplaysRepositoryV04` — null placements, no SQL changes

`ReplaysRepositoryV04` already calls `x.ToDomain()` which, after the change in §4, will return `Placements = null`. No further changes are needed to `GetAll()`, `GetByLeagueId()`, `Create()`, or `Update()` — `Update()` receives the whole `Replay` record but only reads the explicitly listed fields in its SQL; the new field is simply never referenced.

### 6. New `ReplaysRepositoryV05`

File: `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs`

This is a standalone class — no inheritance from `ReplaysRepositoryV04`. Copy-and-adapt pattern.

Key design decisions:

**`GetAll()` and `GetByLeagueId()`**: Fetch replays from the `replays` table, then fetch all `replay_placements` rows for those replay IDs in one second query, and join in C#.

```csharp
// GetAll() sketch
var dbObjects = connection.Query<ReplayDb>(
    "SELECT id, name, status, filename, fullLog, "
    + "league_id AS LeagueId, date AS Date, winner AS Winner, teams AS Teams "
    + "FROM replays");
var ids = dbObjects.Select(r => r.Id).ToList();
var placements = ids.Count > 0
    ? connection.Query<ReplayPlacementDb>(
        "SELECT replay_id AS ReplayId, machine AS Machine, team_name AS TeamName, position AS Position "
        + "FROM replay_placements WHERE replay_id = ANY(@ids)",
        new { ids = ids.ToArray() })
          .ToLookup(p => p.ReplayId)
    : Enumerable.Empty<ReplayPlacementDb>().ToLookup(p => p.ReplayId);
return [.. dbObjects.Select(r => r.ToDomain(placements[r.Id].Select(p => p.ToDomain()).ToList()))];
```

For `GetByLeagueId()`, the same pattern applies — fetch replays for the league, then fetch their placements.

**`ReplayPlacementDb`** mapping record, internal to `ReplaysRepositoryV05.cs`:

```csharp
private sealed record ReplayPlacementDb(int ReplayId, string Machine, string TeamName, int Position)
{
    public ReplayPlacement ToDomain() => new(Machine, TeamName, Position);
}
```

**`ReplayDb.ToDomain()` overload for V05**: Rather than modifying `ReplayDb.cs`, `ReplaysRepositoryV05` can call a local helper. Since `ReplayDb` is `internal sealed` and its `ToDomain()` method returns `Placements = null`, the V05 class passes the pre-fetched placements list when constructing the `Replay`:

```csharp
// Inside ReplaysRepositoryV05, after fetching db:
return db.ToDomain() with { Placements = placementList };
```

This works because `Replay` is a record and `with` expressions can override any property.

**`Update()` — transactional write**:

```csharp
public void Update(Replay item)
{
    ArgumentNullException.ThrowIfNull(item);
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    const string replaySql = "UPDATE replays SET "
        + "name = @name, status = @status, filename = @filename, fullLog = @fullLog, "
        + "league_id = @leagueId, date = @date, winner = @winner, teams = @teams "
        + "WHERE id = @id";
    _ = connection.Execute(replaySql, new
    {
        id = int.Parse(item.Id, CultureInfo.InvariantCulture),
        name = item.Name, status = item.Status, filename = item.Filename,
        fullLog = item.FullLog, leagueId = item.LeagueId, date = item.Date,
        winner = item.Winner, teams = item.Teams?.ToArray()
    }, transaction);

    var replayId = int.Parse(item.Id, CultureInfo.InvariantCulture);

    _ = connection.Execute(
        "DELETE FROM replay_placements WHERE replay_id = @replayId",
        new { replayId }, transaction);

    if (item.Placements is { Count: > 0 })
    {
        foreach (var p in item.Placements)
        {
            _ = connection.Execute(
                "INSERT INTO replay_placements (replay_id, machine, team_name, position) "
                + "VALUES (@replayId, @machine, @teamName, @position)",
                new { replayId, machine = p.Machine, teamName = p.TeamName, position = p.Position },
                transaction);
        }
    }

    transaction.Commit();
}
```

If `item.Placements` is `null` (called by the backfill for replays with no log), the DELETE still runs (a no-op) and nothing is inserted — which is correct for idempotency. If the transaction rolls back (any exception), neither the replay row nor the placement rows are committed, and the exception propagates to `Processor.UpdateReplay()`, which does not catch it — so the queue message is not deleted and becomes visible again for retry.

**`Create()`**: Same SQL as `ReplaysRepositoryV04.Create()`. Placements are never written at creation time.

**Static constructor**: Copy the `SqlMapper.AddTypeHandler(StringArrayHandler.Instance)` static constructor from `ReplaysRepositoryV04`. The `StringArrayHandler` type is already `internal sealed` in `ReplayDb.cs` and is accessible within the same assembly.

### 7. `ServiceRegistration` in Hub Storage

Add a second version boundary:

```csharp
private static readonly Version PlacementsMinVersion = new(0, 5);
private static readonly Version ReplayLeagueFieldsMinVersion = new(0, 4);

public static IServiceCollection AddHubStorageServices(this IServiceCollection builder) =>
    builder.AddSingleton<DatabaseSchemaVersion>()
        .AddScoped<IRepository<Game>, GamesRepository>()
        .AddScoped<IReplaysRepository>(sp =>
        {
            var version = sp.GetRequiredService<DatabaseSchemaVersion>()
                .GetCurrentVersionAsync().GetAwaiter().GetResult();
            if (version is not null && version >= PlacementsMinVersion)
            {
                return new ReplaysRepositoryV05(sp.GetRequiredService<IConfiguration>());
            }
            if (version is not null && version >= ReplayLeagueFieldsMinVersion)
            {
                return new ReplaysRepositoryV04(sp.GetRequiredService<IConfiguration>());
            }
            return new ReplaysRepository(sp.GetRequiredService<IConfiguration>());
        })
        // ... rest unchanged
```

### 8. `IFeatureFlags` and `GatewayFeatureFlags`

Add to `IFeatureFlags`:

```csharp
Task<bool> IsPlacementsEnabledAsync();
```

Add to `GatewayFeatureFlags`:

```csharp
private static readonly Version PlacementsMinVersion = new(0, 5);

public async Task<bool> IsPlacementsEnabledAsync()
{
    var current = await schemaVersion.GetCurrentVersionAsync();
    return current is not null && current >= PlacementsMinVersion;
}
```

### 9. `ServiceRegistration` in Gateway — register `IFeatureFlags` in worker path

In `AddWorkerServices()`, add `IFeatureFlags` registration so `PlacementsBackfillService` can depend on it even in distributed (worker-only) mode:

```csharp
public static IServiceCollection AddWorkerServices(this IServiceCollection builder) =>
    builder.AddHubStorageServices()
        .AddQueueServices()
        .AddWormsArmageddonFilesServices()
        .AddHttpClient()
        .AddScoped<Processor>()
        .AddScoped<IAnnouncer, Announcer>()
        .TryAddScoped<IFeatureFlags, GatewayFeatureFlags>()  // <-- add, using TryAdd to avoid double-registration in monolith mode
```

Note: `TryAddScoped` is an extension method from `Microsoft.Extensions.DependencyInjection`. The `using` directive for `Microsoft.Extensions.DependencyInjection` is already present in the file (via implicit usings or direct using). Use the full method form:

```csharp
builder.TryAddScoped<IFeatureFlags, GatewayFeatureFlags>();
return builder;
```

Since the existing method uses a fluent chain (`builder.AddX().AddY()...`), split the `TryAddScoped` call out to a statement and return `builder` at the end, or adjust the chain to a series of `_ = builder.AddX()` statements.

### 10. `Processor.cs` — map placements

Inside `Processor.UpdateReplay()`, extend the `with` expression:

```csharp
var updatedReplay = replay with
{
    Status = "Processed",
    FullLog = replayLog,
    Date = replayModel.Date == default ? null : replayModel.Date,
    Winner = string.IsNullOrEmpty(replayModel.Winner) ? null : replayModel.Winner,
    Teams = replayModel.Teams.Count > 0
        ? replayModel.Teams.Select(t => t.Name).ToList()
        : null,
    Placements = replayModel.Placements
        .Select(p => new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position))
        .ToList()
};
```

`replayModel.Placements` is `IReadOnlyCollection<Placement>` (always non-null, may be empty). Mapping produces a `List<ReplayPlacement>` (which is `IReadOnlyList<ReplayPlacement>`). An empty list is a valid result — the repository will write zero placement rows.

Add `using Worms.Hub.Storage.Domain;` if not already present (it is not in the current file — add it).

### 11. `PlacementsBackfillService`

File: `src/Worms.Hub.Gateway/Worker/PlacementsBackfillService.cs`

This is an `internal sealed class PlacementsBackfillService : BackgroundService`. It uses a scoped service provider to resolve dependencies per-run (matching the pattern of `CheckForMessagesService`).

Behaviour:
1. `IsPlacementsEnabledAsync()` → if false, log and return.
2. Check `replay_placements` table row count. If > 0, log "backfill already complete" and return.
3. Fetch all replays with `Status = "Processed"` via `IReplaysRepository.GetAll()`.
4. For each replay:
   - If `replay.FullLog` is null or empty, skip silently (log at debug level).
   - Parse `replay.FullLog` via `IReplayTextReader.GetModel(fullLog)`.
   - Map `replayModel.Placements` → `IReadOnlyList<ReplayPlacement>`.
   - Call `replayRepository.Update(replay with { Placements = placements })`.
   - If `Update()` throws, log the error and continue.
5. Log completion.

To check if `replay_placements` is empty, inject `IConfiguration` and open a Dapper connection directly — or better, depend on `IReplaysRepository` and note that `GetAll()` returns placements, but that doesn't tell us whether the placements table is empty vs. no replays exist. To correctly determine "table is empty", query the table directly.

Use `IConfiguration` to open an `NpgsqlConnection` and run:
```sql
SELECT COUNT(*) FROM replay_placements
```
If the count > 0, skip.

Alternatively, inject `DatabaseSchemaVersion` for the feature flag check and `IConfiguration` for the direct count query. Since `IFeatureFlags` wraps `DatabaseSchemaVersion` and is already registered in the worker path (§9), prefer injecting `IFeatureFlags`.

Constructor:
```csharp
internal sealed class PlacementsBackfillService(
    IServiceProvider serviceProvider,
    ILogger<PlacementsBackfillService> logger) : BackgroundService
```

Inside `ExecuteAsync`:
```csharp
using var scope = serviceProvider.CreateScope();
var featureFlags = scope.ServiceProvider.GetRequiredService<IFeatureFlags>();
var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
var replayRepository = scope.ServiceProvider.GetRequiredService<IReplaysRepository>();
var replayTextReader = scope.ServiceProvider.GetRequiredService<IReplayTextReader>();
```

Check `replay_placements` count with a direct Dapper query using `IConfiguration.GetConnectionString("Database")` — this avoids adding a dedicated repository method for a one-time startup check.

Telemetry: Start an `Activity` from `Telemetry.Source` named `"Placement Backfill"` with `ActivityKind.Internal`.

### 12. Register `PlacementsBackfillService` in `Program.cs`

Inside the `if (runWorker)` block, alongside the existing `CheckForMessagesService` registration:

```csharp
if (runWorker)
{
    _ = builder.Services.AddWorkerServices();
    _ = builder.Services.AddHostedService<CheckForMessagesService>();
    _ = builder.Services.AddHostedService<PlacementsBackfillService>();
}
```

`BackgroundService` implementations are registered as singletons by `AddHostedService`. `PlacementsBackfillService` creates its own scope so scoped services (`IFeatureFlags`, `IReplaysRepository`, etc.) resolve correctly.

### 13. `ReplaysController.cs` — fix positional Replay construction

The existing call:
```csharp
repository.Create(new Replay("0", parameters.Name, "Pending", tempFilename, null, "redgate", null, null, null));
```
Has 9 arguments. After adding `Placements`, `Replay` has 10 positional parameters. Add `null` as the 10th:
```csharp
repository.Create(new Replay("0", parameters.Name, "Pending", tempFilename, null, "redgate", null, null, null, null));
```

### 14. Testing

The testing strategy doc states: prefer integration tests against a real Postgres for database behaviour; avoid mocking the data layer. The existing pattern has no unit-test projects for Hub Storage or Gateway.

For this slice, no new test projects are added. Correctness is verified by:
- Build passing with `make cli.build` (catches all positional Replay construction sites and missing interface method implementations).
- `dotnet build --warnaserror` against both `Worms.Hub.Storage` and `Worms.Hub.Gateway`.
- Manual verification against `docker compose up` stack (described in the Verification section below).

---

## Verification

1. `make cli.build` — confirms the solution builds cleanly with no warnings-as-errors. This catches any missed `new Replay(...)` call sites and confirms `ReplaysRepositoryV05` compiles and satisfies `IReplaysRepository`.

2. `dotnet build src/Worms.Hub.Storage --warnaserror` and `dotnet build src/Worms.Hub.Gateway --warnaserror` — surface any Roslyn/NullAnalysis issues.

3. Bring up the local stack with `docker compose up`. Confirm Flyway applies `V0.5` cleanly (check the Flyway container logs for `Successfully applied 1 migration`).

4. Upload a replay via the CLI (or directly via `curl`) and wait for the Worker to process it. Query the database: `SELECT * FROM replay_placements;` — confirm one row per team with the correct `machine`, `team_name`, and `position`.

5. Confirm tied positions: if two teams were eliminated on the same turn, verify they share a `position` value.

6. Deploy (or configure) the Worker against a schema < V0.5 (i.e., roll back `V0.5__AddReplayPlacements.sql`) and process a replay. Confirm the Worker completes without error and the `replay_placements` table does not exist (or is empty if it exists).

7. Start the Worker against a schema ≥ V0.5 with at least one `Processed` replay and an empty `replay_placements` table. Check the Worker startup logs for "Placement backfill complete" (or equivalent). Then query `replay_placements` — confirm rows are present for each team in each processed replay.

8. Start the Worker a second time. Confirm the logs show "backfill already complete — skipping" (or equivalent) and no duplicate rows are written.

9. Verify that a replay with `fullLog IS NULL` in the database is skipped by the backfill without causing an error log (only a debug-level skip entry).
