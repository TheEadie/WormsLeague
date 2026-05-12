# Plan: League List

## Context

This slice delivers the first real data page of the SPA: a signed-in member navigates to `/leagues` and sees a card for every league in the database. It also introduces the `leagues` table to the database schema, wires a new `GET /api/v1/leagues` list endpoint in the Hub Gateway, updates the existing `GET /api/v1/leagues/{id}` endpoint to 404 when the id is absent from the database, seeds "redgate" for local-dev, and removes the placeholder `/authenticated` route and page now that it has a proper destination.

It builds directly on the foundations from earlier slices:
- Slice 09 (`authenticated-route-gate`) delivered `RequireAuth` — every authenticated route wraps children in that component.
- Slice 08 (`browser-sign-in`) delivered `CallbackPage`, `auth.ts`, and the OIDC flow — the only change here is the post-callback redirect target.
- Slice 04 (`gateway-cors`) configured CORS on the Gateway — no change needed.
- Slice 01 (`spa-scaffolding`) delivered `make web.build` / `make web.lint` and the CI jobs — no build system changes are needed.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/database/migrations/V0.3__AddLeagues.sql` | Flyway migration that creates the `leagues` table (id, name columns) |
| `src/database/local-dev/R__LeaguesTestData.sql` | Repeatable seed that inserts the "redgate" league row |
| `src/Worms.Hub.Storage/Database/LeaguesRepository.cs` | Dapper repository; `GetAll()` returns all rows, `GetById()` returns one row or null |
| `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx` | SPA page: fetches `/api/v1/leagues`, renders cards, handles loading/error |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Storage/ServiceRegistration.cs` | Register `LeaguesRepository` as `Scoped` |
| `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` | Add `GET` (list) action; update `GET {id}` to 404 when id absent from DB |
| `src/Worms.Hub.Web/src/pages/CallbackPage.tsx` | Change redirect target from `/authenticated` to `/leagues` |
| `src/Worms.Hub.Web/src/App.tsx` | Add `/leagues` route wrapped in `RequireAuth`; remove `/authenticated` route and its import |
| `src/Worms.Hub.Web/src/pages/AuthenticatedPage.tsx` | Delete this file (placeholder removed) |

---

## Implementation Details

### 1. Database migration — `leagues` table

Create `src/database/migrations/V0.3__AddLeagues.sql`:

```sql
CREATE TABLE IF NOT EXISTS public.leagues
(
    id   text NOT NULL,
    name text NOT NULL,
    CONSTRAINT leagues_pkey PRIMARY KEY (id)
);
```

The `id` column is `text` (not auto-generated) because league ids are human-readable identifiers like `"redgate"` — consistent with how `SchemeFiles.GetLatestDetails` already uses the id as a filesystem name. The `name` column stores the display name; in the initial seed it will match the id.

Migration version `V0.3` follows the existing sequence (`V0.2.2` is the highest). Flyway's `validateMigrationNaming = true` is set — use the exact filename pattern: `V<major>.<minor>__<Description>.sql`.

### 2. Local-dev seed data

Create `src/database/local-dev/R__LeaguesTestData.sql`:

```sql
DELETE FROM public.leagues;
INSERT INTO public.leagues (id, name) VALUES ('redgate', 'Redgate');
```

Repeatable scripts (`R__`) are re-run whenever their checksum changes. Using `DELETE` then `INSERT` (rather than `INSERT ... ON CONFLICT`) is consistent with the existing `R__GamesTestData.sql` and `R__ReplaysTestData.sql` scripts.

### 3. Hub Storage — `LeaguesRepository`

Create `src/Worms.Hub.Storage/Database/LeaguesRepository.cs`. This repository does **not** implement `IRepository<League>` because `IRepository<T>` requires `Create` and `Update` which are not needed here. Instead it exposes the two operations the slice requires:

```csharp
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

[PublicAPI]
public sealed class LeaguesRepository(IConfiguration configuration)
{
    public IReadOnlyList<LeagueRecord> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<LeagueRecord>("SELECT id, name FROM leagues")];
    }

    public LeagueRecord? GetById(string id)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return connection.QuerySingleOrDefault<LeagueRecord>(
            "SELECT id, name FROM leagues WHERE id = @id", new { id });
    }
}

[PublicAPI]
public record LeagueRecord(string Id, string Name);
```

Notes:
- `LeagueRecord` is a new flat DB record type. The existing `League` domain type (which carries `Version` and `SchemePath` from the filesystem) is assembled by the controller, not stored in the DB.
- Following the pattern from `GamesRepository`/`ReplaysRepository`: mark with `[PublicAPI]`, use `internal sealed class` for the repository (but `[PublicAPI]` elevates visibility for DI discovery — follow exactly the same pattern as the other repositories; `GamesRepository` is `internal sealed` but its record `GamesDb` is `public`. Mirror this — make `LeaguesRepository` `internal sealed` and expose `LeagueRecord` as `public`).

Actually, review the existing code: `GamesRepository` is `internal sealed` but `GamesDb` is annotated `[PublicAPI]` and declared `public`. `ReplaysRepository` is `internal sealed` and `ReplayDb` is `[PublicAPI] public`. Follow the same: make `LeaguesRepository` `internal sealed` and `LeagueRecord` `[PublicAPI] public`.

Revised:

```csharp
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

internal sealed class LeaguesRepository(IConfiguration configuration)
{
    public IReadOnlyList<LeagueRecord> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<LeagueRecord>("SELECT id, name FROM leagues")];
    }

    public LeagueRecord? GetById(string id)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return connection.QuerySingleOrDefault<LeagueRecord>(
            "SELECT id, name FROM leagues WHERE id = @id", new { id });
    }
}

[PublicAPI]
public record LeagueRecord(string Id, string Name);
```

Register in `ServiceRegistration.cs` by adding `.AddScoped<LeaguesRepository>()` to the chain.

### 4. Gateway — update `LeaguesController`

Replace the entire file `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Worms.Hub.Gateway.API.DTOs;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.API.Controllers;

internal sealed class LeaguesController(SchemeFiles schemeFiles, LeaguesRepository leaguesRepository) : V1ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeagueDto>>> GetAll()
    {
        var dbLeagues = leaguesRepository.GetAll();
        var tasks = dbLeagues.Select(async dbLeague =>
        {
            var latestDetails = await schemeFiles.GetLatestDetails(dbLeague.Id);
            return LeagueDto.FromDomain(
                latestDetails,
                new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id = dbLeague.Id })!, UriKind.Relative));
        });
        var results = await Task.WhenAll(tasks);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeagueDto>> Get(string id)
    {
        var dbLeague = leaguesRepository.GetById(id);
        if (dbLeague is null)
        {
            return NotFound();
        }

        var latestDetails = await schemeFiles.GetLatestDetails(id);
        return LeagueDto.FromDomain(
            latestDetails,
            new Uri(Url.Action(action: "Get", controller: "SchemeFiles", values: new { id })!, UriKind.Relative));
    }
}
```

The list action is `[HttpGet]` (no template) so it maps to `GET /api/v1/leagues`. The `[Authorize]` is inherited from `V1ApiController`. Return type for the list is `ActionResult<IReadOnlyList<LeagueDto>>` — `Ok(results)` satisfies this. The 404 path in `Get(string id)` is only triggered when the id is absent from the `leagues` table; the filesystem fetch still happens after the DB check.

### 5. SPA — `LeagueListPage.tsx`

Create `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx`. This page:
- Calls `GET /api/v1/leagues` with the bearer token from `useAuth`.
- Shows `CircularProgress` while loading (when `leagues === null && error === null`).
- Shows an error message if the fetch fails.
- Shows a `Card` for each league (name, id, scheme version), each wrapped in a `Link` to `/leagues/{id}`.

```tsx
import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { Link } from 'react-router'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import CircularProgress from '@mui/material/CircularProgress'
import Card from '@mui/material/Card'
import CardActionArea from '@mui/material/CardActionArea'
import CardContent from '@mui/material/CardContent'
import { gatewayUrl } from '../api'

interface LeagueDto {
    id: string
    name: string
    version: string
    schemeUrl: string
}

function LeagueListPage() {
    const auth = useAuth()
    const [leagues, setLeagues] = useState<LeagueDto[] | null>(null)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!auth.user?.access_token) return
        fetch(`${gatewayUrl}/api/v1/leagues`, {
            headers: { Authorization: `Bearer ${auth.user.access_token}` },
        })
            .then((res) => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`)
                return res.json() as Promise<LeagueDto[]>
            })
            .then(setLeagues)
            .catch((err: unknown) => setError(String(err)))
    }, [auth.user?.access_token])

    return (
        <Box sx={{ p: 4 }}>
            <Typography variant="h4" sx={{ mb: 3 }}>
                Leagues
            </Typography>
            {leagues === null && error === null && <CircularProgress />}
            {error !== null && (
                <Typography color="error">Error loading leagues: {error}</Typography>
            )}
            {leagues !== null && leagues.length === 0 && (
                <Typography color="text.secondary">No leagues found.</Typography>
            )}
            {leagues !== null && leagues.length > 0 && (
                <Box
                    sx={{
                        display: 'grid',
                        gap: 2,
                        gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' },
                    }}
                >
                    {leagues.map((league) => (
                        <Card key={league.id} variant="outlined">
                            <CardActionArea component={Link} to={`/leagues/${league.id}`}>
                                <CardContent>
                                    <Typography variant="h6">{league.name}</Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        {league.id}
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        v{league.version}
                                    </Typography>
                                </CardContent>
                            </CardActionArea>
                        </Card>
                    ))}
                </Box>
            )}
        </Box>
    )
}

export default LeagueListPage
```

**`CardActionArea` with `Link` — typing caveat:** MUI's `CardActionArea` accepts a `component` prop that changes the underlying element. Passing `component={Link}` from `react-router` will work at runtime. TypeScript may complain about `to` not being in `CardActionArea`'s props when using the standard MUI types. The safe approach is to use `sx` and wrap the card in `<Link>` without using the `component` override pattern:

```tsx
<Link to={`/leagues/${league.id}`} style={{ textDecoration: 'none' }}>
    <Card variant="outlined" sx={{ '&:hover': { boxShadow: 3 } }}>
        <CardContent>
            ...
        </CardContent>
    </Card>
</Link>
```

Use the `<Link>` wrapper approach — it avoids the MUI `component` prop typing issue and is simpler.

The `version` field in `LeagueDto` from the Gateway is a `System.Version` serialised as `"1.0.0.0"` (or similar dot-separated string). Display it directly. No parsing needed on the frontend.

### 6. SPA — `CallbackPage.tsx` redirect update

Change line 14 from:

```tsx
void navigate('/authenticated', { replace: true })
```

to:

```tsx
void navigate('/leagues', { replace: true })
```

### 7. SPA — `App.tsx` routing update

Remove the `/authenticated` route and `AuthenticatedPage` import. Add `/leagues` and `/leagues/:id` routes:

```tsx
import { createBrowserRouter, RouterProvider } from 'react-router'
import Layout from './components/Layout'
import RequireAuth from './components/RequireAuth'
import LandingPage from './pages/LandingPage'
import CallbackPage from './pages/CallbackPage'
import LeagueListPage from './pages/LeagueListPage'

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            { index: true, element: <LandingPage /> },
            { path: 'callback', element: <CallbackPage /> },
            {
                path: 'leagues',
                element: (
                    <RequireAuth>
                        <LeagueListPage />
                    </RequireAuth>
                ),
            },
            {
                path: 'leagues/:id',
                element: (
                    <RequireAuth>
                        <div>League detail — coming soon</div>
                    </RequireAuth>
                ),
            },
        ],
    },
])

function App() {
    return <RouterProvider router={router} />
}

export default App
```

The `/leagues/:id` route is a placeholder (dead link per spec); wrapping it in `RequireAuth` is correct so that signed-out users hitting a direct URL are still redirected to `/`.

Prettier will expand the multi-line JSX in the route definitions — run `npx prettier --write src` before `make web.lint`, as per prior slice learnings.

### 8. Delete `AuthenticatedPage.tsx`

Delete `src/Worms.Hub.Web/src/pages/AuthenticatedPage.tsx`. It is no longer referenced after the `App.tsx` changes.

### 9. Flyway — schema model (optional, low-priority)

The `src/database/schema-model/` directory contains `.rgm` files managed by Redgate Flyway Desktop. These are not required for the migration to apply or tests to pass. Do not create or modify schema model files; they are updated by the Flyway Desktop tooling, not by hand.

---

## Verification

1. `make web.build` — must succeed (TypeScript compile + Vite bundle). Fix any TypeScript errors in `LeagueListPage.tsx` or `App.tsx` before proceeding.
2. Run `npx prettier --write src` from `src/Worms.Hub.Web/` to normalise formatting, then `make web.lint` — ESLint, `tsc --noEmit`, and Prettier check must all pass.
3. `dotnet build src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj --warnaserror` — must compile with no warnings. This validates the controller changes and the new `LeaguesRepository` registration.
4. `dotnet build src/Worms.Hub.Storage/Worms.Hub.Storage.csproj --warnaserror` — must compile with no warnings. This validates `LeaguesRepository.cs`.
5. `docker compose up` — apply migrations with Flyway and bring up the gateway. Confirm the `leagues` table exists in Postgres and contains a `redgate` row.
6. Authenticated GET to `http://localhost:5005/api/v1/leagues` — must return HTTP 200 with `[{"id":"redgate","name":"Redgate",...}]` (requires scheme files for "redgate" to be present at the configured `Storage:SchemesFolder`).
7. Authenticated GET to `http://localhost:5005/api/v1/leagues/nonexistent` — must return HTTP 404.
8. Unauthenticated GET to `http://localhost:5005/api/v1/leagues` — must return HTTP 401.
9. In the browser at `http://localhost:5173/leagues` (signed in) — league cards appear, each showing name, id, and scheme version; clicking a card navigates to `/leagues/redgate`.
10. In the browser at `http://localhost:5173/leagues` (signed out) — redirected to `/` by `RequireAuth`.
11. After completing the OIDC sign-in flow (click Sign In on the landing page, complete auth) — browser lands on `/leagues`, not `/authenticated`.
