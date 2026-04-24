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
2. Create a `XxxRepository : IRepository<Xxx>` in `Database/`, including an internal `XxxDb` record for Dapper mapping.
3. Register in `ServiceRegistration.AddHubStorageServices()`.
4. Add a migration in `src/database/migrations/` (Flyway versioned migration, e.g. `V0.3__AddXxx.sql`).
