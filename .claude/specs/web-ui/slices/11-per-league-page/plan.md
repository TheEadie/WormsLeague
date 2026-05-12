# Plan: Per-League Page

## Context

This slice delivers a `/leagues/{id}` page where a signed-in member can see the league name, scheme version, scheme download link, and a list of historic replays with date, winner, and team names. It builds on the League List slice (10), which introduced `LeaguesRepository`, `LeagueDto`, `LeaguesController` (with `GET /api/v1/leagues` and `GET /api/v1/leagues/{id}`), and the `/leagues` route in the SPA. This slice extends four areas:

1. **Database** — adds `league_id`, `date`, `winner`, `teams` columns to `replays`, backfills them for existing rows, and updates the local-dev seed.
2. **Storage** — extends the `Replay` domain record and `ReplaysRepository` to carry the new fields; adds a `GetByLeagueId` query method.
3. **Gateway** — adds `GET /api/v1/leagues/{id}/replays` on `LeaguesController`, returns `ReplayInLeagueDto`, returns 404 when the league does not exist.
4. **SPA** — replaces the `leagues/:id` placeholder route with a real `LeagueDetailPage` that concurrently fetches the league and its replays.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/database/migrations/V0.4__AddReplayLeagueFields.sql` | Adds `league_id`, `date`, `winner`, `teams` columns to `replays` with FK constraint |
| `src/database/migrations/V0.4.1__BackfillReplayLeagueFields.sql` | Sets `league_id = 'redgate'` for all rows; extracts `date`, `winner`, `teams` from `full_log` using regex for rows that have it |
| `src/Worms.Hub.Gateway/API/DTOs/ReplayInLeagueDtos.cs` | `ReplayInLeagueDto` record returned by the new endpoint |
| `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` | The new per-league SPA page |

### Modified files

| Path | Change |
|---|---|
| `src/database/local-dev/R__ReplaysTestData.sql` | Add at least one replay row with `league_id = 'redgate'`, populated `date`, `winner`, and `teams` |
| `src/Worms.Hub.Storage/Domain/Replay.cs` | Add `LeagueId`, `Date`, `Winner`, `Teams` nullable properties |
| `src/Worms.Hub.Storage/Database/ReplaysRepository.cs` | Update all SQL + mapping to include the four new columns; add `GetByLeagueId(string leagueId)` method; update `ReplayDb` record |
| `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` | Add `GetReplays(string id)` action returning 404 / 200 |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Set `Date`, `Winner`, `Teams` on the updated replay from the parsed `replayModel` |
| `src/Worms.Hub.Web/src/App.tsx` | Replace the `leagues/:id` placeholder element with `<LeagueDetailPage />` |

---

## Implementation Details

### 1. Database migration — V0.4 (column additions)

File: `src/database/migrations/V0.4__AddReplayLeagueFields.sql`

```sql
ALTER TABLE public.replays
    ADD COLUMN IF NOT EXISTS league_id text,
    ADD COLUMN IF NOT EXISTS date      timestamp,
    ADD COLUMN IF NOT EXISTS winner    text,
    ADD COLUMN IF NOT EXISTS teams     text[];

ALTER TABLE public.replays
    ADD CONSTRAINT replays_leagues_fk
    FOREIGN KEY (league_id) REFERENCES public.leagues (id);
```

All four columns are nullable — existing rows have no league association until the backfill migration runs. The `teams` column is a PostgreSQL text array (`text[]`), which Npgsql / Dapper maps naturally to `string[]` or `IReadOnlyList<string>`.

### 2. Database migration — V0.4.1 (backfill)

File: `src/database/migrations/V0.4.1__BackfillReplayLeagueFields.sql`

```sql
-- Set league for all existing rows
UPDATE public.replays SET league_id = 'redgate';

-- Backfill date, winner, teams from full_log for rows that have it
UPDATE public.replays
SET
    date = (
        regexp_match(fulllog, 'Game Started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) GMT')
    )[1]::timestamp
WHERE fulllog IS NOT NULL;

UPDATE public.replays
SET
    winner = CASE
        WHEN fulllog ~ 'The round was drawn\.' THEN 'Draw'
        ELSE (regexp_match(fulllog, '(.+) wins the (?:match!|round\.)'))[1]
    END
WHERE fulllog IS NOT NULL;

UPDATE public.replays
SET
    teams = ARRAY(
        SELECT DISTINCT
            COALESCE(
                (regexp_match(m, 'Colour: ".+" as "(.+)"'))[1],
                (regexp_match(m, 'Colour: "(.+)"'))[1]
            )
        FROM unnest(regexp_matches(fulllog,
            E'(?:Colour: ".+" as ".+"|Colour: ".+")', 'g')) AS m
    )
WHERE fulllog IS NOT NULL;
```

Note: `regexp_matches` with the `'g'` flag returns a set; wrapping with `ARRAY(...)` collects them. The two patterns match online (`"player" as "team"`) and offline (`"team"`) formats. The `COALESCE` inside picks the group that matched.

> **Caveat:** The regex approach for `teams` is mildly complex in pure SQL. The migration should be tested against the `docker compose` Postgres instance before treating it as correct. An acceptable alternative is to split the team extraction into two separate UPDATE statements — one for the online pattern, one for the offline pattern — and use `array_cat` or a union approach.

### 3. Local-dev seed update

File: `src/database/local-dev/R__ReplaysTestData.sql`

Replace the current minimal insert with:

```sql
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-05-01',
    'Processed',
    'seed_replay.WAgame',
    'redgate',
    '2024-05-01 19:30:00',
    'Team Alpha',
    ARRAY['Team Alpha', 'Team Beta']
) ON CONFLICT DO NOTHING;

INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams)
VALUES (
    '2024-04-15',
    'Pending',
    'seed_replay2.WAgame',
    'redgate',
    NULL,
    NULL,
    NULL
) ON CONFLICT DO NOTHING;
```

This gives the per-league page at least one processed replay (with date/winner/teams) and one unprocessed replay (showing the holding message), covering both branches of the UI.

Note: `R__` prefix means Flyway re-runs this script on every migration run (repeatable migration), so `ON CONFLICT DO NOTHING` guards against duplicate inserts. The existing `R__ReplaysTestData.sql` already uses a plain `INSERT` without conflict handling — update it to add `ON CONFLICT DO NOTHING` for safety, or simply replace the row with the two inserts above.

### 4. Storage domain — Replay record

File: `src/Worms.Hub.Storage/Domain/Replay.cs`

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
    IReadOnlyList<string>? Teams);
```

The record is `public sealed` (already public in the codebase). Add the four new nullable fields at the end so any existing `new Replay(...)` calls remain valid positionally — but check for positional callers and update them; the primary caller is `ReplaysRepository.GetAll()` and `Processor.cs`.

### 5. Storage repository — ReplaysRepository

File: `src/Worms.Hub.Storage/Database/ReplaysRepository.cs`

**Key changes:**

- `GetAll()` SELECT: add `league_id AS LeagueId, date AS Date, winner AS Winner, teams AS Teams` and map the four new columns in the projection.
- `Create()` INSERT: include `league_id, date, winner, teams` in the column list and parameters.
- `Update()` SET clause: include `league_id = @leagueId, date = @date, winner = @winner, teams = @teams`.
- New method:

```csharp
public IReadOnlyList<Replay> GetByLeagueId(string leagueId)
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    const string sql = "SELECT id, name, status, filename, fullLog, league_id AS LeagueId, "
        + "date AS Date, winner AS Winner, teams AS Teams "
        + "FROM replays WHERE league_id = @leagueId ORDER BY date DESC NULLS LAST";
    var dbObjects = connection.Query<ReplayDb>(sql, new { leagueId });
    return [.. dbObjects.Select(MapToDomain)];
}
```

Extract a private `MapToDomain(ReplayDb x)` helper to avoid duplicating the seven-field mapping in `GetAll()` and `GetByLeagueId()`.

Update `ReplayDb` at the bottom of the file:

```csharp
[PublicAPI]
public record ReplayDb(
    int Id,
    string Name,
    string Status,
    string Filename,
    string? FullLog,
    string? LeagueId,
    DateTime? Date,
    string? Winner,
    string[]? Teams);
```

Npgsql maps PostgreSQL `text[]` to `string[]` natively. Expose it as `IReadOnlyList<string>?` in the domain record — cast `string[]` to `IReadOnlyList<string>` in the mapping helper.

`ReplaysRepository` is `internal sealed` (matching the pattern of `GamesRepository`). It is consumed via `IRepository<Replay>` from the Gateway, so its visibility stays `internal`. The new `GetByLeagueId` method is not part of `IRepository<T>` — inject `ReplaysRepository` directly into `LeaguesController` as a concrete type (the same pattern as `LeaguesRepository` is injected).

> **Note from learnings (slice 10):** types injected directly as concrete types (not via an interface) across assembly boundaries must be `public`. `ReplaysRepository` is currently `internal sealed`. It is registered as `IRepository<Replay>` and the `IRepository<T>` interface is `public` — the concrete class itself does not need to be public for that registration. However, since `LeaguesController` will now also inject `ReplaysRepository` directly for `GetByLeagueId`, and it is in a different assembly, `ReplaysRepository` must be promoted to `public sealed`.

### 6. Gateway — ReplayInLeagueDtos

File: `src/Worms.Hub.Gateway/API/DTOs/ReplayInLeagueDtos.cs`

```csharp
using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record ReplayInLeagueDto(
    string Id,
    string Name,
    bool Processed,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams)
{
    internal static ReplayInLeagueDto FromDomain(Replay replay) =>
        new(
            replay.Id,
            replay.Name,
            replay.Date.HasValue && replay.Winner != null && replay.Teams != null,
            replay.Date,
            replay.Winner,
            replay.Teams);
}
```

`Processed` is `true` when all three of `date`, `winner`, and `teams` are present.

### 7. Gateway — LeaguesController new action

File: `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs`

Add `ReplaysRepository replaysRepository` to the primary constructor and a new action:

```csharp
[HttpGet("{id}/replays")]
public ActionResult<IReadOnlyList<ReplayInLeagueDto>> GetReplays(string id)
{
    var league = leaguesRepository.GetById(id);
    if (league is null)
    {
        return NotFound();
    }
    var replays = replaysRepository.GetByLeagueId(id);
    return Ok(replays.Select(ReplayInLeagueDto.FromDomain).ToList());
}
```

No `async/await` needed — `GetByLeagueId` is synchronous Dapper. The `[Authorize]` attribute is inherited from `V1ApiController`.

### 8. Worker Processor — store date, winner, teams

File: `src/Worms.Hub.Gateway/Worker/Processor.cs`

After `var replayModel = replayTextReader.GetModel(replayLog);`, update the `updatedReplay` construction:

```csharp
var updatedReplay = replay with
{
    Status = "Processed",
    FullLog = replayLog,
    Date = replayModel.Date == default ? null : replayModel.Date,
    Winner = string.IsNullOrEmpty(replayModel.Winner) ? null : replayModel.Winner,
    Teams = replayModel.Teams.Count > 0
        ? replayModel.Teams.Select(t => t.Name).ToList()
        : null
};
```

`replayModel.Date` is a `DateTime` (not nullable); use `default` as the sentinel for "not parsed". `replayModel.Teams` is `IReadOnlyCollection<Team>` — project to team names only.

### 9. SPA — LeagueDetailPage

File: `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx`

Two interfaces matching the API responses:

```typescript
interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
}

interface ReplayInLeagueDto {
    id: string
    name: string
    processed: boolean
    date: string | null      // ISO 8601 string from JSON
    winner: string | null
    teams: string[] | null
}
```

State:

```typescript
const { id } = useParams<{ id: string }>()
const auth = useAuth()
const [league, setLeague] = useState<LeagueDto | null>(null)
const [replays, setReplays] = useState<ReplayInLeagueDto[] | null>(null)
const [error, setError] = useState<string | null>(null)
const [notFound, setNotFound] = useState(false)
```

Data fetching: use `Promise.all` to fire both requests concurrently in a single `useEffect` keyed on `[auth.user?.access_token, id]`.

```typescript
useEffect(() => {
    if (!auth.user?.access_token || !id) return
    const token = auth.user.access_token
    const headers = { Authorization: `Bearer ${token}` }
    Promise.all([
        fetch(`${gatewayUrl}/api/v1/leagues/${id}`, { headers }),
        fetch(`${gatewayUrl}/api/v1/leagues/${id}/replays`, { headers }),
    ])
        .then(async ([leagueRes, replaysRes]) => {
            if (leagueRes.status === 404) {
                setNotFound(true)
                return
            }
            if (!leagueRes.ok) throw new Error(`HTTP ${leagueRes.status}`)
            if (!replaysRes.ok) throw new Error(`HTTP ${replaysRes.status}`)
            const [leagueData, replaysData] = await Promise.all([
                leagueRes.json() as Promise<LeagueDto>,
                replaysRes.json() as Promise<ReplayInLeagueDto[]>,
            ])
            setLeague(leagueData)
            setReplays(replaysData)
        })
        .catch((err: unknown) => setError(String(err)))
}, [auth.user?.access_token, id])
```

Page render structure (inside a `<Container maxWidth="xl" sx={{ py: { xs: 2, md: 4 } }}>` to match the league list page):

- **Loading:** `league === null && replays === null && error === null && !notFound` → `<CircularProgress />`
- **Not found:** `notFound` → `<Typography>League not found.</Typography>`
- **Error:** `error !== null` → `<Typography color="error">Error: {error}</Typography>`
- **Content:** when both `league` and `replays` are non-null:
  - `<Typography variant="h4" sx={{ fontWeight: 700 }}>{league.name}</Typography>`
  - Scheme section: if `league.version` is non-null, show `<Chip label={\`Scheme v${league.version}\`} .../>` and a `<Link href={league.schemeUrl} ...>Download scheme</Link>` (use MUI `Link` from `@mui/material/Link` for styling, `component="a"` with `target="_blank"`).
  - Replay list or empty state.

**Replay list:** render as a `<Stack spacing={1}>` of `<Paper variant="outlined">` rows, each wrapped in a React Router `<Link>` to `/leagues/${id}/replays/${replay.id}`:

- **Processed row** (`replay.processed === true`): date (formatted as a readable string from the ISO timestamp), winner team name, and team names joined or shown as chips.
- **Unprocessed row** (`replay.processed === false`): `replay.name` and a muted message such as `"Processing in progress — check back later."`.
- **Empty state:** `replays.length === 0` → `<Typography color="text.secondary">No replays found for this league.</Typography>`

Date formatting: use `new Date(replay.date!).toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' })` inline.

Keep styling consistent with `LeagueListPage` — use `monoFontFamily` from `../theme` for date/id values, `CircularProgress` for loading, `Container` for outer layout.

### 10. SPA — App.tsx routing update

File: `src/Worms.Hub.Web/src/App.tsx`

- Add `import LeagueDetailPage from './pages/LeagueDetailPage'` at the top.
- Replace:
  ```tsx
  { path: 'leagues/:id', element: (<RequireAuth><div>League detail — coming soon</div></RequireAuth>) }
  ```
  with:
  ```tsx
  { path: 'leagues/:id', element: (<RequireAuth><LeagueDetailPage /></RequireAuth>) }
  ```

---

## Verification

1. Run `dotnet build --warnaserror src/Worms.Hub.Storage/Worms.Hub.Storage.csproj` — must pass after adding the four new fields to `Replay`, `ReplayDb`, and `ReplaysRepository`.
2. Run `dotnet build --warnaserror src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj` — must pass after adding `ReplayInLeagueDto`, the new controller action, and the processor update.
3. Run `make web.build && make web.lint` — must pass after adding `LeagueDetailPage.tsx` and updating `App.tsx`.
4. Run `docker compose up` and navigate to `/leagues/redgate` in the browser. Confirm:
   - The heading shows "Redgate".
   - The scheme version chip and download link are visible.
   - At least one processed replay row shows a date, a winner team name, and participating team names.
   - At least one unprocessed replay row shows the replay name and the holding message.
5. In the browser or with `curl` (using a valid bearer token): `GET /api/v1/leagues/redgate/replays` returns HTTP 200 with a non-empty array.
6. `GET /api/v1/leagues/nonexistent/replays` returns HTTP 404.
7. `GET /api/v1/leagues/redgate/replays` without an `Authorization` header returns HTTP 401.
8. Navigate to `/leagues/nonexistent-league` in the browser — confirm the "League not found" message is displayed, not a blank or broken page.
9. Navigate to `/leagues/redgate` while not signed in — confirm redirect to `/`.
10. Verify each replay row is a link (clicking navigates to `/leagues/redgate/replays/{id}`, which will be a dead link until slice 12 is implemented — this is expected).
