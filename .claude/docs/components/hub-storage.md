# Hub Storage Component

Project: `Worms.Hub.Storage`

## Domain models

Domain objects are C# `record` types in `Domain/`:

- `Game(string Id, string Status, string HostMachine)`
- `Replay(string Id, string Name, string Status, string Filename, string? FullLog)`
- `League(...)` and `CliInfo(...)` — supporting types

Records use `with` expressions for updates. IDs come from the database and are stored as strings (parsed/formatted with `CultureInfo.InvariantCulture`).

## Repository pattern

`IRepository<T>` provides three operations:

```csharp
IReadOnlyCollection<T> GetAll();
T Create(T item);
void Update(T item);
```

Implementations (`GamesRepository`, `ReplaysRepository`) use **Dapper** + **Npgsql**. Each method creates a fresh `NpgsqlConnection` from `IConfiguration.GetConnectionString("Database")` — connections are not pooled at the application level (Npgsql handles connection pooling internally).

SQL is written as inline string constants. Column names match exactly between SQL and the `[PublicAPI] record XxxDb(...)` mapping type defined at the bottom of each repository file. The DB record type is separate from the domain record to avoid Dapper's column mapping interfering with the domain model.

## File storage classes

File abstractions in `Files/` wrap filesystem operations and derive their paths from `IConfiguration`:

| Class | Config key | Purpose |
|---|---|---|
| `ReplayFiles` | `Storage:TempReplayFolder` | Save uploaded replays, locate `.log` sidecars |
| `SchemeFiles` | `Storage:SchemesFolder` | Read `.wsc` scheme files from disk |
| `CliFiles` | `Storage:CliFolder` | Serve CLI binaries |
| `GameFiles` | `Storage:GameFolder` | Access WA game files |

`ReplayFiles.SaveFileContents()` writes the incoming stream to a randomly named file (via `Path.GetRandomFileName()`), creating the folder if needed. The generated filename is stored in the DB and used to locate the file later.

`ReplayFiles.GetLogPath()` looks for a `.log` sidecar alongside the replay file (same name, different extension). Returns `null` if not found.

## Service registration

`AddHubStorageServices()` registers all repositories and file classes as `Scoped`.

## Adding a new domain object

1. Add a `record` in `Domain/`.
2. Create a `XxxRepository : IRepository<Xxx>` in `Database/`. Include a `[PublicAPI] record XxxDb(...)` for Dapper column mapping — the type **must** be named `XxxDb` (matching `GamesDb`, `ReplayDb`). The `[PublicAPI]` annotation marks it as resolved reflectively by Dapper/DI and suppresses false-positive unused-code warnings. Use `internal sealed` for both the repository and the DB record unless the repository will be injected by concrete type from another assembly (e.g. the Gateway), in which case the repository class must be `public sealed`.
3. Register in `ServiceRegistration.AddHubStorageServices()`.
4. Add a migration in `src/database/migrations/` (Flyway versioned migration, e.g. `V0.3__AddXxx.sql`).

## Schema compatibility

When a slice adds columns to an **existing** table and the gateway reads those columns, the plan must include an explicit compatibility decision before implementation:

- **Require DB upgrade first:** the gateway may crash if deployed against the old schema. Only acceptable if gateway and DB are always upgraded together.
- **Degrade gracefully:** gate the new endpoint or query behind a `DatabaseSchemaVersion` check. The DI-factory pattern selects between a base repository implementation and a versioned subtype at startup based on the detected schema version, so controllers never reference `DatabaseSchemaVersion` directly.

Any plan that extends a repository to read new columns must document which approach is chosen and enumerate all write paths (controllers, workers, tests) that construct or persist the affected record type, confirming each sets the new field correctly.
