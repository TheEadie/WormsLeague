# Plan: Alias Claiming — Standalone Page

## Context

This slice introduces player identity and team alias management. It builds on the three earlier slices
that (1) extended the replay parser with finish-position data, (2) persisted that data into the
`replay_placements` table, and (3) displayed placements in the UI. With placements already stored,
this slice adds `players` and `teams` tables (V0.7 migration), repositories and a upsert hook in
the worker, two new API endpoints, and a standalone `/teams` Web UI page where authenticated users
can claim or unclaim `(machine, team name)` pairs. No ELO logic is included; that is the next slice.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/database/migrations/V0.7__AddPlayersAndTeams.sql` | Creates `players` and `teams` tables; backfills `teams` from `replay_placements` |
| `src/Worms.Hub.Storage/Domain/Player.cs` | `Player` domain record |
| `src/Worms.Hub.Storage/Domain/Team.cs` | `Team` domain record (with `ClaimedByPlayerName` and `ClaimedByAuth0Subject`) |
| `src/Worms.Hub.Storage/Database/IPlayersRepository.cs` | Public interface for player persistence |
| `src/Worms.Hub.Storage/Database/PlayersRepository.cs` | Dapper/Npgsql implementation |
| `src/Worms.Hub.Storage/Database/ITeamsRepository.cs` | Public interface for teams persistence |
| `src/Worms.Hub.Storage/Database/TeamsRepository.cs` | Dapper/Npgsql implementation |
| `src/Worms.Hub.Gateway/API/DTOs/TeamDtos.cs` | `TeamDto` and `ClaimTeamDto` request/response records |
| `src/Worms.Hub.Gateway/API/Controllers/TeamsController.cs` | `GET /teams` and `PUT /teams/{id}` |
| `src/Worms.Hub.Web/src/pages/TeamsPage.tsx` | Standalone `/teams` page |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Storage/ServiceRegistration.cs` | Register `IPlayersRepository` and `ITeamsRepository` as `Scoped` |
| `src/Worms.Hub.Gateway/FeatureFlags/IFeatureFlags.cs` | Add `IsTeamsEnabledAsync()` |
| `src/Worms.Hub.Gateway/FeatureFlags/FeatureFlags.cs` | Implement `IsTeamsEnabledAsync()` with `TeamsMinVersion = new(0, 7)` |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Inject `ITeamsRepository`; upsert team pairs after replay update, gated on `IsTeamsEnabledAsync()` |
| `src/Worms.Hub.Web/src/App.tsx` | Add `/teams` route wrapped in `<RequireAuth>` |

---

## Implementation Details

### 1. Database migration

File: `src/database/migrations/V0.7__AddPlayersAndTeams.sql`

```sql
CREATE TABLE IF NOT EXISTS public.players (
    id            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    auth0_subject text    NOT NULL UNIQUE,
    display_name  text    NOT NULL
);

CREATE TABLE IF NOT EXISTS public.teams (
    id          integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    machine     text    NOT NULL,
    team_name   text    NOT NULL,
    player_id   integer REFERENCES public.players (id),
    UNIQUE (machine, team_name)
);

INSERT INTO public.teams (machine, team_name)
SELECT DISTINCT machine, team_name
FROM public.replay_placements
ON CONFLICT DO NOTHING;
```

Flyway picks up migrations in version order. The latest migration before this is V0.6, so V0.7 is the next file. No sub-version suffix is needed.

### 2. Domain records

**`src/Worms.Hub.Storage/Domain/Player.cs`**

```csharp
using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Player(int Id, string Auth0Subject, string DisplayName);
```

**`src/Worms.Hub.Storage/Domain/Team.cs`**

```csharp
using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record Team(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedByPlayerName,
    string? ClaimedByAuth0Subject);
```

`ClaimedByPlayerName` and `ClaimedByAuth0Subject` are both null when the team is unclaimed. The controller uses `ClaimedByAuth0Subject` to derive `isMyTeam` rather than doing an extra lookup.

### 3. Repository interfaces

**`src/Worms.Hub.Storage/Database/IPlayersRepository.cs`**

```csharp
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IPlayersRepository
{
    Player? GetByAuth0Subject(string auth0Subject);
    Player Create(Player player);
}
```

**`src/Worms.Hub.Storage/Database/ITeamsRepository.cs`**

```csharp
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface ITeamsRepository
{
    IReadOnlyCollection<Team> GetAll();
    Team? GetById(int id);
    void Upsert(string machine, string teamName);
    void SetPlayerClaim(int teamId, int? playerId);
}
```

`SetPlayerClaim` sets `player_id` to the given value (null to unclaim). Business-logic checks (who owns the team, whether to 409 or 403) live in the controller, not the repository. This keeps the repository as a thin data layer consistent with the rest of the codebase.

### 4. Repository implementations

**`src/Worms.Hub.Storage/Database/PlayersRepository.cs`**

```csharp
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class PlayersRepository(IConfiguration configuration) : IPlayersRepository
{
    public Player? GetByAuth0Subject(string auth0Subject)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var db = connection.QuerySingleOrDefault<PlayerDb>(
            "SELECT id, auth0_subject AS Auth0Subject, display_name AS DisplayName "
            + "FROM players WHERE auth0_subject = @auth0Subject",
            new { auth0Subject });
        return db is null ? null : new Player(db.Id, db.Auth0Subject, db.DisplayName);
    }

    public Player Create(Player player)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        const string sql =
            "INSERT INTO players (auth0_subject, display_name) "
            + "VALUES (@auth0Subject, @displayName) RETURNING id";
        var id = connection.QuerySingle<int>(sql,
            new { auth0Subject = player.Auth0Subject, displayName = player.DisplayName });
        return player with { Id = id };
    }
}

[PublicAPI]
public record PlayerDb(int Id, string Auth0Subject, string DisplayName);
```

Note: `PlayersRepository` is `public sealed` because it is injected by interface from the Gateway assembly.

**`src/Worms.Hub.Storage/Database/TeamsRepository.cs`**

```csharp
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class TeamsRepository(IConfiguration configuration) : ITeamsRepository
{
    private const string SelectSql =
        "SELECT t.id AS Id, t.machine AS Machine, t.team_name AS TeamName, "
        + "p.display_name AS ClaimedByPlayerName, p.auth0_subject AS ClaimedByAuth0Subject "
        + "FROM teams t LEFT JOIN players p ON t.player_id = p.id";

    public IReadOnlyCollection<Team> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<TeamDb>(SelectSql).Select(MapToDomain)];
    }

    public Team? GetById(int id)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        var db = connection.QuerySingleOrDefault<TeamDb>(SelectSql + " WHERE t.id = @id", new { id });
        return db is null ? null : MapToDomain(db);
    }

    public void Upsert(string machine, string teamName)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        _ = connection.Execute(
            "INSERT INTO teams (machine, team_name) VALUES (@machine, @teamName) "
            + "ON CONFLICT DO NOTHING",
            new { machine, teamName });
    }

    public void SetPlayerClaim(int teamId, int? playerId)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        _ = connection.Execute(
            "UPDATE teams SET player_id = @playerId WHERE id = @teamId",
            new { teamId, playerId });
    }

    private static Team MapToDomain(TeamDb db) =>
        new(db.Id, db.Machine, db.TeamName, db.ClaimedByPlayerName, db.ClaimedByAuth0Subject);
}

[PublicAPI]
public record TeamDb(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedByPlayerName,
    string? ClaimedByAuth0Subject);
```

Both `TeamsRepository` and `PlayersRepository` are `public sealed`. `TeamsRepository` is `public sealed` for the same reason — injected by the Gateway via the interface.

### 5. ServiceRegistration — Hub Storage

Add the two new repositories to the end of the `AddHubStorageServices()` chain:

```csharp
.AddScoped<IPlayersRepository, PlayersRepository>()
.AddScoped<ITeamsRepository, TeamsRepository>()
```

No version-gated factory is needed; the feature flag gate at the controller and worker level is sufficient. The migration creates the tables at startup, so the repositories are always safe to register.

### 6. Feature flag

**`IFeatureFlags.cs`** — add one method:

```csharp
Task<bool> IsTeamsEnabledAsync();
```

**`FeatureFlags.cs`** — add the version constant and implementation:

```csharp
private static readonly Version TeamsMinVersion = new(0, 7);

public async Task<bool> IsTeamsEnabledAsync()
{
    var current = await schemaVersion.GetCurrentVersionAsync();
    return current is not null && current >= TeamsMinVersion;
}
```

### 7. DTOs

**`src/Worms.Hub.Gateway/API/DTOs/TeamDtos.cs`**

```csharp
using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record TeamDto(
    int Id,
    string Machine,
    string TeamName,
    string? ClaimedBy,
    bool IsMyTeam)
{
    internal static TeamDto FromDomain(Team team, string? callerAuth0Subject) =>
        new(team.Id,
            team.Machine,
            team.TeamName,
            team.ClaimedByPlayerName,
            team.ClaimedByAuth0Subject is not null
                && team.ClaimedByAuth0Subject == callerAuth0Subject);
}

[PublicAPI]
internal sealed record ClaimTeamDto(bool Claimed);
```

### 8. TeamsController

**`src/Worms.Hub.Gateway/API/Controllers/TeamsController.cs`**

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Gateway.FeatureFlags;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class TeamsController(
    ITeamsRepository teamsRepository,
    IPlayersRepository playersRepository,
    IFeatureFlags featureFlags) : V1ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll()
    {
        if (!await featureFlags.IsTeamsEnabledAsync())
        {
            return NotFound();
        }

        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teams = teamsRepository.GetAll();
        return Ok(teams.Select(t => TeamDto.FromDomain(t, callerSubject)).ToList());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] ClaimTeamDto body)
    {
        if (!await featureFlags.IsTeamsEnabledAsync())
        {
            return NotFound();
        }

        var team = teamsRepository.GetById(id);
        if (team is null)
        {
            return NotFound();
        }

        var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (body.Claimed)
        {
            if (team.ClaimedByAuth0Subject is not null
                && team.ClaimedByAuth0Subject != callerSubject)
            {
                return Conflict();
            }

            var player = playersRepository.GetByAuth0Subject(callerSubject!);
            if (player is null)
            {
                var displayName = ResolveDisplayName();
                player = playersRepository.Create(new Player(0, callerSubject!, displayName));
            }

            teamsRepository.SetPlayerClaim(id, player.Id);
        }
        else
        {
            if (team.ClaimedByAuth0Subject is not null
                && team.ClaimedByAuth0Subject != callerSubject)
            {
                return Forbid();
            }

            teamsRepository.SetPlayerClaim(id, null);
        }

        return Ok();
    }

    private string ResolveDisplayName()
    {
        var nickname = User.FindFirstValue("nickname");
        var name = User.FindFirstValue("name");
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return nickname ?? name ?? sub ?? "Unknown";
    }
}
```

Key points:
- Inherits `V1ApiController` which sets `[ApiVersion("1.0")]`, `[Route("api/v{version:apiVersion}/[controller]")]`, and `[Authorize(Roles = "access")]`.
- `ClaimTypes.NameIdentifier` resolves to `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`, which is what the `Auth:NameClaim` config maps to — confirmed from `appsettings.json`. This gives the Auth0 `sub` value.
- Display name resolution: `nickname ?? name ?? sub ?? "Unknown"`. Auth0 JWTs include `nickname` and `name` claims as-is. The `?? "Unknown"` fallback guards against the theoretically impossible case where `sub` is also null (shouldn't happen for a valid token, but required to compile safely).
- No `[HttpGet]` attribute on `GetAll()` — add it explicitly. In contrast to `LeaguesController` (which shows the pattern), use `[HttpGet]` on the `GetAll` action.
- `Forbid()` returns 403; `Conflict()` returns 409.
- The `callerSubject!` null-forgive: the `[Authorize]` attribute on the base controller guarantees the user is authenticated, so the sub claim is always present. Still assign defensively with `?? "Unknown"` in `ResolveDisplayName`.

### 9. Worker Processor — teams upsert

In `Processor.cs`, add `ITeamsRepository teamsRepository` to the constructor parameter list. After the `replayRepository.Update(updatedReplay)` call (before the announcer call), add:

```csharp
if (await featureFlags.IsTeamsEnabledAsync())
{
    foreach (var placement in replayModel.Placements)
    {
        teamsRepository.Upsert(placement.Team.Machine, placement.Team.Name);
    }
}
```

This is gated on the feature flag so it is a no-op before V0.7 is applied. `replayModel.Placements` is the list from the parsed log; `placement.Team.Machine` and `placement.Team.Name` come from the `ReplayResource` model already used in the existing `updatedReplay` assignment.

The `ITeamsRepository` is available in the worker scope because `AddWorkerServices()` calls `AddHubStorageServices()`, which registers it.

### 10. Web UI — TeamsPage.tsx

File: `src/Worms.Hub.Web/src/pages/TeamsPage.tsx`

The page:
1. On mount (when `auth.user?.access_token` is available), fetches `GET /api/v1/teams`.
2. Sorts the list: unclaimed first, then `isMyTeam === true`, then claimed by others. Within each group: alphabetically by `machine`, then `teamName`.
3. Renders a MUI `Table` with columns: Machine, Team Name, Status/Action.
4. Unclaimed rows: Claim button.
5. `isMyTeam` rows: Unclaim button.
6. Other claimed rows: "Claimed by {claimedBy}" text, no button.
7. Buttons are disabled during in-flight requests (`pending` state per row id).
8. On failure: show inline error on the row, re-enable button.
9. Error messages: 409 → "Already claimed by another player"; 403 → "You don't own this team"; other → "Something went wrong, please try again".
10. Empty state: "No teams found. Teams appear here once replays have been processed."
11. Load failure: generic error message instead of table.

Key implementation details:
- Use `useState<Map<number, string>>` for per-row error messages (keyed by team id).
- Use `useState<Set<number>>` for in-flight team ids (the `pending` set).
- After a successful claim/unclaim, re-fetch the full list (simple approach consistent with existing pages that re-fetch on state change).
- Import MUI components: `Box`, `Button`, `CircularProgress`, `Container`, `Paper`, `Table`, `TableBody`, `TableCell`, `TableContainer`, `TableHead`, `TableRow`, `Typography` from `@mui/material`.
- No new npm dependencies are needed; all required MUI components are already installed.
- The outermost `<Container>` must use `sx={{ flex: 1 }}` on the containing `Box` to satisfy the layout requirement (page components inside Layout must size with `flex: 1`).

Sorting function:

```typescript
function sortGroup(team: TeamDto): number {
    if (!team.claimedBy) return 0
    if (team.isMyTeam) return 1
    return 2
}

const sorted = [...teams].sort((a, b) => {
    const groupDiff = sortGroup(a) - sortGroup(b)
    if (groupDiff !== 0) return groupDiff
    const machineDiff = a.machine.localeCompare(b.machine)
    if (machineDiff !== 0) return machineDiff
    return a.teamName.localeCompare(b.teamName)
})
```

Claim/Unclaim handler:

```typescript
async function handleClaim(id: number, claimed: boolean) {
    setPending((prev) => new Set(prev).add(id))
    setErrors((prev) => { const m = new Map(prev); m.delete(id); return m })
    try {
        const res = await fetch(`${gatewayUrl}/api/v1/teams/${id}`, {
            method: 'PUT',
            headers: {
                Authorization: `Bearer ${auth.user!.access_token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ claimed }),
        })
        if (!res.ok) {
            const msg =
                res.status === 409 ? 'Already claimed by another player'
                : res.status === 403 ? "You don't own this team"
                : 'Something went wrong, please try again'
            setErrors((prev) => new Map(prev).set(id, msg))
        } else {
            await loadTeams()
        }
    } catch {
        setErrors((prev) => new Map(prev).set(id, 'Something went wrong, please try again'))
    } finally {
        setPending((prev) => { const s = new Set(prev); s.delete(id); return s })
    }
}
```

The `loadTeams` function is extracted into a `useCallback` (or called directly from inside the `useEffect`) so it can also be called after a successful mutation.

Interface for the DTO:

```typescript
interface TeamDto {
    id: number
    machine: string
    teamName: string
    claimedBy: string | null
    isMyTeam: boolean
}
```

### 11. App.tsx — add /teams route

Add an import for `TeamsPage` and insert the route alongside the existing auth-gated routes:

```typescript
import TeamsPage from './pages/TeamsPage'

// inside the router:
{
    path: 'teams',
    element: (
        <RequireAuth>
            <TeamsPage />
        </RequireAuth>
    ),
},
```

This route is not added to the header nav (out of scope per spec).

### 12. Scope decision — list endpoint asymmetry

`GET /teams` (list) and `PUT /teams/{id}` (single) are both new in this slice. No existing endpoint serves `Team` data. There is no asymmetry to resolve with an existing list endpoint. The list endpoint returns all teams globally (no league scoping), which is correct per spec.

### 13. Caveats from prior learnings

- **`internal sealed record` is required** for all new C# record types to satisfy CA1852 (from slice 03 learnings). The domain records `Player` and `Team` are `public sealed record`; the Dapper DB records `PlayerDb` and `TeamDb` are `public record` (accessed reflectively by Dapper from another assembly via `[PublicAPI]`). Mark them `public record` to satisfy the public-API requirement without `sealed` — Dapper needs reflective access and they have no subtypes in practice. Actually, the `[PublicAPI]` annotation suppresses the "can be sealed" analyser warning only when the type is truly reflectively-used. To be safe, declare them `public sealed record` and annotate with `[PublicAPI]` — consistent with how `GamesDb` is declared in `GamesRepository.cs` (line 57: `[PublicAPI] public record GamesDb(...)`). Follow that exact pattern: `[PublicAPI] public record PlayerDb(...)` and `[PublicAPI] public record TeamDb(...)` without `sealed`, consistent with the existing `GamesDb` and `ReplayDb` pattern.
- **`using Microsoft.Extensions.DependencyInjection.Extensions;`** must be present wherever `TryAddScoped` is used (from slice 02 learnings). No `TryAddScoped` is added in this slice, so this is not a concern.
- **Prettier formatting** must be run after writing `.tsx` files (from slice 03 learnings). Run `npx prettier --write src` inside `src/Worms.Hub.Web/` before committing.
- **`tsc -b` not `tsc --noEmit`** must be used to type-check the web project (from web component doc).
- **`make web.lint` must pass** before committing web changes.

---

## Verification

1. **Migration applies cleanly**: `docker compose up` applies V0.7; confirm `players` and `teams` tables exist and `teams` is backfilled from `replay_placements` by querying both tables.
2. **Build passes**: `dotnet build --warnaserror src/Worms.Hub.Gateway` and `dotnet build --warnaserror src/Worms.Hub.Storage` both exit 0.
3. **Web lint/type-check passes**: `make web.lint` exits 0 after running `npx prettier --write src` inside `src/Worms.Hub.Web/`.
4. **Web build passes**: `make web.build` exits 0.
5. **GET /teams returns 404 before migration**: start the gateway against a database at schema < V0.7; `GET /api/v1/teams` returns 404.
6. **GET /teams returns populated list**: with V0.7 applied and processed replays present, `GET /api/v1/teams` with a valid bearer token returns a JSON array with `id`, `machine`, `teamName`, `claimedBy` (null), `isMyTeam` (false).
7. **GET /teams returns empty list**: with V0.7 applied and no replays processed, returns `[]` with HTTP 200.
8. **PUT claim success**: call `PUT /api/v1/teams/{id}` with `{"claimed":true}` as an authenticated user; response is 200 empty body; subsequent `GET /teams` shows `claimedBy` and `isMyTeam: true` for that team; exactly one player row exists.
9. **PUT claim idempotent**: calling `PUT ... {"claimed":true}` again for the same team/user returns 200; no duplicate player row.
10. **PUT claim conflict**: claim a team as user A; call `PUT ... {"claimed":true}` as user B; response is 409.
11. **PUT unclaim success**: after claiming, call `PUT ... {"claimed":false}`; response is 200; `GET /teams` shows `claimedBy: null` and `isMyTeam: false`.
12. **PUT unclaim forbidden**: try to unclaim a team owned by a different user; response is 403.
13. **PUT not found**: `PUT /api/v1/teams/99999` returns 404.
14. **Worker upsert**: process a new replay containing a team not already in `teams`; after processing, `SELECT * FROM teams WHERE team_name = '<new team>'` returns a row.
15. **Web page renders**: navigate to `/teams` in a browser while authenticated; the page shows the sorted list with Claim/Unclaim buttons and "Claimed by" text as appropriate.
16. **Web auth guard**: navigate to `/teams` while unauthenticated; redirected to `/`.
