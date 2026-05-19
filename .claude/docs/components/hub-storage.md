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

A new repository method belongs on the repository for the aggregate it **returns**, not the table it reads from. A method returning leagues lives on `ILeaguesRepository` even if its SQL reads from the replays table; if the right repository does not exist yet, introduce it rather than parking the method on an unrelated one. Plans for new queries name the returned aggregate explicitly; reviewers challenge any query whose return type does not match its host repository.

When a slice adds new fields to a domain entity that already has an aggregate `Update(entity)` write, the default is to extend that write rather than introduce field-targeted repo methods (`UpdateXxxField`, `ClearXxxField`). Bespoke per-field methods on the same aggregate are a smell — they fragment writes into N round-trips, open partial-read windows between the writes, and almost always shadow what the aggregate write should already do. Reviewers must check whether the aggregate write could absorb the new fields before accepting a bespoke method.

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

## Natural keys over surrogates

If a column on a new table is already unique, not-null, and is the only key the application looks rows up by, make it the primary key. Do not add a `SERIAL` surrogate alongside it — the surrogate buys nothing and forces foreign keys to be expressed against an integer the application never reads. Reviewers challenge any new table whose surrogate is never read by application code.

## Local-dev seed data

Seed scripts in `src/database/local-dev/` are Flyway repeatable scripts (`R__*.sql`) that run in `docker compose up` and on demand when re-seeded. Two rules apply:

- **Seed source-of-truth inputs, not derived state.** Replays, teams, claims, and other inputs to the runtime calculators belong in seed data. Placements, ratings, leaderboard snapshots, and anything else a backfiller or processor computes on startup do not — let the runtime code populate the derived rows on first run. Hardcoding both produces drift the moment the calculator changes.
- **Use Worms-themed names.** Teams take weapon/animal puns (Blitz Brigade, Banana Boys, Holy Rollers, Mad Bombers); players take in-game character or weapon references (BazookaJoe, Concrete Donkey). Avoid scaffolding placeholders like `Team Alpha` or `Other Player`.

Repeatable scripts run in alphabetical order. If a seed script needs to clear tables before reseeding, use `TRUNCATE ... CASCADE` rather than `DELETE`, because foreign-key dependents (e.g. replays referencing leagues) will be cleared before their parents otherwise. Plans that add a new seed surface must include the seed-data update needed to exercise that surface; coverage means including 2-/3-/4-team variations, draws, and pending replays where the new surface would otherwise hide behind a single happy-path scenario.

## Schema compatibility

When a slice adds columns to an **existing** table and the gateway reads those columns, the plan must include an explicit compatibility decision before implementation:

- **Require DB upgrade first:** the gateway may crash if deployed against the old schema. Only acceptable if gateway and DB are always upgraded together.
- **Degrade gracefully:** gate the new endpoint or query behind a runtime schema-version check, returning a fallback (e.g. empty list, 503) until the migration has run. A DI-factory pattern that picks between a base repository and a versioned subtype at startup keeps the version check out of controller code.

Any plan that extends a repository to read new columns must document which approach is chosen and enumerate all write paths (controllers, workers, tests) that construct or persist the affected record type, confirming each sets the new field correctly.
