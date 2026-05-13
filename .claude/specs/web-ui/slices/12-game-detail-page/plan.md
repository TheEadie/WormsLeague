# Plan: Game Detail Page

## Context

This slice adds a game detail page — the drill-down from the per-league replay list that was delivered in slice 11. It consists of two parts: a new `GET /api/v1/leagues/{id}/replays/{replayId}` endpoint in the Hub Gateway that parses the stored `full_log` on demand and returns a turn-by-turn breakdown, and a new `GameDetailPage` in the SPA that shows a hero card with stats, a breadcrumb, and a two-panel sidebar layout (Turn-by-turn and Weapons). Slice 11 already delivers `LeagueDetailPage` which navigates to `/leagues/:id/replays/:replayId` on row click, so the routing hook is already present; this slice just needs to register the route in `App.tsx` and create the page component.

`IReplayTextReader` already exists in `Worms.Armageddon.Files` and is registered via `AddWormsArmageddonFilesServices()`. That registration is currently only called from `AddWorkerServices()`. The gateway HTTP pipeline does not yet call it, so it must be wired in for the gateway path before `LeaguesController` can use it.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx` | The full game detail SPA page (hero card, breadcrumb, sidebar panels) |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` | Add `GET {id}/replays/{replayId}` action that looks up replay, parses log, returns `ReplayDetailDto` |
| `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs` | Add `ReplayDetailDto`, `TurnDto`, `WeaponDto`, `DamageSummaryDto` records |
| `src/Worms.Hub.Gateway/ServiceRegistration.cs` | Add `AddWormsArmageddonFilesServices()` call inside `AddGatewayServices()` so `IReplayTextReader` is available when the gateway runs without the worker |
| `src/Worms.Hub.Web/src/App.tsx` | Register `/leagues/:id/replays/:replayId` route |
| `src/database/local-dev/R__ReplaysTestData.sql` | Add one replay row whose `full_log` column contains a realistic WA log string |

---

## Implementation Details

### 1. New DTO types in `ReplayDtos.cs`

Add three new record types alongside the existing `ReplayDto` and `CreateReplayDto`:

```csharp
[PublicAPI]
internal sealed record ReplayDetailDto(
    string Id,
    string Name,
    string Status,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams,
    IReadOnlyList<TurnDto>? Turns);

[PublicAPI]
internal sealed record TurnDto(
    int TurnNumber,
    string TeamName,
    IReadOnlyList<WeaponDto> Weapons,
    IReadOnlyList<DamageSummaryDto> Damage);

[PublicAPI]
internal sealed record WeaponDto(string Name);

[PublicAPI]
internal sealed record DamageSummaryDto(
    string TeamName,
    uint HealthLost,
    uint WormsKilled);
```

`ReplayDetailDto` does not extend `ReplayDto` — it's a separate flat record to keep the JSON shape clean and explicit.

Add a static factory method `ReplayDetailDto.FromDomain(Replay replay, ReplayResource? parsed)`:

```csharp
internal static ReplayDetailDto FromDomain(Replay replay, ReplayResource? parsed)
{
    IReadOnlyList<TurnDto>? turns = null;

    if (parsed is not null)
    {
        var turnList = parsed.Turns.Select((t, i) => new TurnDto(
            i + 1,
            t.Team.Name,
            t.Weapons.Select(w => new WeaponDto(w.Name)).ToList(),
            t.Damage.Select(d => new DamageSummaryDto(d.Team.Name, d.HealthLost, d.WormsKilled)).ToList()
        )).ToList();
        turns = turnList.Count > 0 ? turnList : null;
    }

    return new ReplayDetailDto(
        replay.Id,
        replay.Name,
        replay.Status,
        replay.Date,
        replay.Winner,
        replay.Teams,
        turns);
}
```

Note: `turns` is set to `null` (not an empty list) when `parsed` has no turns, so the SPA can distinguish "no log" from "log with zero turns" using the same null check.

### 2. Gateway service registration: add `IReplayTextReader` to gateway path

`AddGatewayServices()` in `src/Worms.Hub.Gateway/ServiceRegistration.cs` must call `AddWormsArmageddonFilesServices()`. Currently only `AddWorkerServices()` does this. The `Worms.Armageddon.Files` project reference already exists in `Worms.Hub.Gateway.csproj` (it is used transitively by the worker build), so no new project reference is needed.

Change `AddGatewayServices()`:

```csharp
public static IServiceCollection AddGatewayServices(this IServiceCollection builder) =>
    builder.AddHttpClient()
        .AddWormsArmageddonFilesServices()
        .AddScoped<IAnnouncer, Announcer>()
        .AddScoped<ReplayFileValidator>()
        .AddScoped<CliFileValidator>()
        .AddScoped<IFeatureFlags, GatewayFeatureFlags>();
```

This is safe because `AddWormsArmageddonFilesServices` registers with `AddScoped` and `ServiceCollection` silently ignores duplicate scoped registrations — but verify that `AddWorkerServices` also calls it for the worker path. `AddWorkerServices` already calls `AddWormsArmageddonFilesServices()` independently; if both run in the same process (monolith mode), DI will have duplicate `IReplayLineParser` registrations. Check whether `IReplayTextReader` uses `IEnumerable<IReplayLineParser>` (it does — it takes `IEnumerable<IReplayLineParser>` in its constructor). Duplicate registrations of `IReplayLineParser` would cause each parser to run twice per line. To avoid this, conditionally add only in gateway mode, or extract the check.

**Safer approach**: Call `AddWormsArmageddonFilesServices()` once in `Program.cs` outside both `AddGatewayServices` and `AddWorkerServices`, only when the gateway is running — then remove it from `AddWorkerServices`. However, that restructures `Program.cs` more broadly.

**Simplest safe approach**: Move the call out of both methods and into `Program.cs`, called unconditionally (it's needed by both the gateway and the worker):

In `Program.cs`, on the line:
```csharp
_ = builder.Services.AddHubStorageServices().AddGatewayServices().AddQueueServices();
```

Change to:
```csharp
_ = builder.Services.AddHubStorageServices().AddGatewayServices().AddQueueServices().AddWormsArmageddonFilesServices();
```

And remove the `AddWormsArmageddonFilesServices()` call from `AddWorkerServices()` in `ServiceRegistration.cs`.

This ensures there is exactly one registration regardless of which modes run.

You need to add the `using Worms.Armageddon.Files;` namespace to `Program.cs`.

### 3. New endpoint in `LeaguesController`

Inject `IReplayTextReader replayTextReader` into `LeaguesController`'s primary constructor, alongside the existing parameters.

Add a new action:

```csharp
[HttpGet("{id}/replays/{replayId}")]
public ActionResult<ReplayDetailDto> GetReplay(string id, string replayId)
{
    var league = leaguesRepository.GetById(id);
    if (league is null)
    {
        return NotFound();
    }

    var replay = replaysRepository.GetByLeagueId(id).FirstOrDefault(r => r.Id == replayId);
    if (replay is null)
    {
        return NotFound();
    }

    ReplayResource? parsed = null;
    if (!string.IsNullOrEmpty(replay.FullLog))
    {
        parsed = replayTextReader.GetModel(replay.FullLog);
    }

    return ReplayDetailDto.FromDomain(replay, parsed);
}
```

This:
- Returns 404 if the league doesn't exist (wrong `{id}`)
- Returns 404 if the replay doesn't exist for that league (wrong `{replayId}`, or replay belongs to a different league)
- Returns hero data with `turns: null` when `FullLog` is absent
- Returns parsed turns when `FullLog` is present
- Returns 401 for unauthenticated requests (via the inherited `[Authorize]` on `V1ApiController`)

The `replayId` coming from the URL is a string; `Replay.Id` is also a string. The comparison `r.Id == replayId` is correct without parsing.

### 4. Route registration in `App.tsx`

Add a new import and route child entry. Place it after the `leagues/:id` route:

```tsx
import GameDetailPage from './pages/GameDetailPage'
```

```tsx
{
    path: 'leagues/:id/replays/:replayId',
    element: (
        <RequireAuth>
            <GameDetailPage />
        </RequireAuth>
    ),
},
```

### 5. `GameDetailPage.tsx` — structure and behaviour

Create `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx`. The page is relatively large; break it into local sub-components within the same file (no separate component files for this slice).

**TypeScript interfaces** (mirrors the gateway DTOs):

```ts
interface WeaponDto {
    name: string
}

interface DamageSummaryDto {
    teamName: string
    healthLost: number
    wormsKilled: number
}

interface TurnDto {
    turnNumber: number
    teamName: string
    weapons: WeaponDto[]
    damage: DamageSummaryDto[]
}

interface ReplayDetailDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
    turns: TurnDto[] | null
}

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
}
```

**Data fetching**: single `useEffect` with `Promise.all` over two fetches — `GET /api/v1/leagues/{id}/replays/{replayId}` and `GET /api/v1/leagues/{id}`. Use the same pattern as `LeagueDetailPage.tsx` (check `res.status === 404` separately; rethrow non-ok responses; parse JSON; set state).

**State**:
- `replay: ReplayDetailDto | null`
- `league: LeagueDto | null`
- `error: string | null`
- `notFound: boolean`
- `activePanel: number` (0 = Turn-by-turn, 1 = Weapons)

**Render logic**:
1. If loading (both null, no error, not notFound): show `<CircularProgress />`.
2. If `notFound`: show "Replay not found." message.
3. If `error !== null`: show error message.
4. If `replay !== null && replay.status !== 'Processed'`: show a "This replay is being processed. Please check back soon." message with no hero card or panels.
5. If `replay !== null && replay.status === 'Processed'` and `league !== null`: show full page.

**Breadcrumb** (when data loaded):
```
Leagues → {league.name} → Match #00x
```
- "Leagues" is `<MuiLink component={RouterLink} to="/leagues">Leagues</MuiLink>`
- "{league.name}" is `<MuiLink component={RouterLink} to={`/leagues/${id}`}>{league.name}</MuiLink>`
- "Match #00x" is `<Typography variant="body2" color="text.primary">Match #{replay.id.padStart(3, '0')}</Typography>`

**Hero card** (a `<Paper variant="outlined" sx={{ p: 3, mb: 2 }}>`):

Title row:
- `<Typography variant="h5" fontWeight={700}>Match #{replay.id.padStart(3, '0')}</Typography>`
- Date and time formatted with `toLocaleDateString` / `toLocaleTimeString` (same locale pattern as `LeagueDetailPage`)
- Winner chip: `color="warning"` when winner is a team name (non-null, non-"Draw"); `color="default"` (neutral) for "Draw". Omit entirely when `replay.winner === null`.
- Chips for participating team names from `replay.teams`.
- Scheme version chip: `label={\`Scheme v${league.version}\`}` — omit when `league.version === null`.

**Stats strip** (four `<Paper variant="outlined">` tiles in a CSS grid `repeat(4, 1fr)`):
- Omit the entire strip when `replay.turns === null || replay.turns.length === 0`.
- **Duration**: `turns[turns.length - 1].end - turns[0].start`. Since the Turn-by-turn data coming from the API has `TurnDto` without timestamp fields (only turn number, weapons, damage), duration **cannot** be computed client-side from the DTO as currently shaped. **Resolution**: the DTO must include timestamps. Add `startMs: number` and `endMs: number` to `TurnDto` (milliseconds from game start as a `double` in .NET, sent as a JSON number). In .NET, `TimeSpan.TotalMilliseconds` gives the double. In the SPA compute duration as `turns[n-1].endMs - turns[0].startMs` ms, then format as `mm:ss`.

  Update `TurnDto` in the gateway:
  ```csharp
  internal sealed record TurnDto(
      int TurnNumber,
      string TeamName,
      double StartMs,
      double EndMs,
      IReadOnlyList<WeaponDto> Weapons,
      IReadOnlyList<DamageSummaryDto> Damage);
  ```

  And in `FromDomain`:
  ```csharp
  new TurnDto(
      i + 1,
      t.Team.Name,
      t.Start.TotalMilliseconds,
      t.End.TotalMilliseconds,
      ...weapons,
      ...damage)
  ```

  Duration helper in the SPA:
  ```ts
  function formatDuration(ms: number): string {
      const totalSeconds = Math.floor(ms / 1000)
      const minutes = Math.floor(totalSeconds / 60)
      const seconds = totalSeconds % 60
      return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
  }
  ```

- **Turns**: `turns.length` (plain number).
- **Max Damage**: `Math.max(...turns.map(t => t.damage.reduce((sum, d) => sum + d.healthLost, 0)))`.
- **Kills**: `turns.reduce((sum, t) => sum + t.damage.reduce((sum2, d) => sum2 + d.wormsKilled, 0), 0)`.

**Sidebar layout** — CSS grid `gridTemplateColumns: '200px 1fr'`:

Left nav (`<Paper variant="outlined">` with a `<List dense>`):
- "Turn-by-turn" (icon: `<TimelineIcon />`), selected when `activePanel === 0`
- "Weapons" (icon: `<GpsFixedIcon />`), selected when `activePanel === 1`
- Use `<ListItemButton selected={activePanel === i} onClick={() => setActivePanel(i)}>`

Right content area: render the active panel component.

**Turn-by-turn panel** (`TurnByTurnPanel` local component):

- When `turns === null || turns.length === 0`: show `<Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>` with an appropriate empty-state message.
- Otherwise: `<TableContainer component={Paper} variant="outlined">` with `<Table size="small">`.
  - Columns: `#`, `Team`, `Weapons used`, `Damage dealt`.
  - Turn number: `String(turn.turnNumber).padStart(2, '0')` in monospace.
  - Team: `<Chip size="small" label={turn.teamName} />`.
  - Weapons: if `turn.weapons.length === 0`, show `—` (em dash in `<Typography variant="caption" color="text.disabled">`). Otherwise render each weapon as a `<Chip size="small" label={w.name} variant="outlined" />` in a `<Stack direction="row" spacing={0.5}>`. The **last** weapon gets `sx={{ fontWeight: 700 }}` (bold label) to visually distinguish it.
  - Damage: if `turn.damage.length === 0`, show `—`. Otherwise for each damage entry: `{d.teamName}: {d.healthLost}` with an optional `<Chip label={`+${d.wormsKilled} kill${d.wormsKilled > 1 ? 's' : ''}`} size="small" color="error" />` when `d.wormsKilled > 0`.

**Weapons panel** (`WeaponsPanel` local component):

- When `turns === null || turns.length === 0`: show empty-state message.
- Otherwise compute per-team weapon stats purely in the component:
  ```ts
  type WeaponStats = { name: string; usageCount: number; attributedDamage: number }
  type TeamWeaponMap = Map<string, WeaponStats[]>
  ```
  Algorithm:
  1. For each turn, find the last weapon (`turn.weapons[turn.weapons.length - 1]`) — this is the "damaging weapon" for attributed damage.
  2. For each turn, for each weapon in `turn.weapons`: increment `usageCount` for `(teamName, weaponName)` pair.
  3. For each turn, sum `turn.damage.reduce((s, d) => s + d.healthLost, 0)` to get total damage for that turn; attribute it to `(teamName, lastWeapon.name)`.
  4. Collect per team, sort entries by `attributedDamage` descending, then `usageCount` descending as tiebreaker.

  Render as a `<Stack spacing={2}>` — one section per team:
  - Team name as a `<Typography variant="subtitle2" fontWeight={700}>`.
  - Weapon entries in a `<TableContainer component={Paper} variant="outlined"><Table size="small">` with columns: Weapon, Uses, Attributed Damage.

### 6. Local-dev seed data in `R__ReplaysTestData.sql`

The file already does `DELETE FROM public.replays` and re-inserts 5 rows. Add a sixth row with `full_log` set to a multi-line WA log. The log must have at least four turns with weapons and damage, a winner line, and team declarations so `IReplayTextReader` can parse it correctly.

Use this canonical seed log (escape the `$` character — PostgreSQL `$$`-quoted strings work without escaping). Use a dollar-quoted string literal to avoid single-quote escaping:

```sql
INSERT INTO public.replays (name, status, filename, league_id, date, winner, teams, fulllog)
VALUES (
    '2024-06-01',
    'Processed',
    'seed_replay_withlog.WAgame',
    'redgate',
    '2024-06-01 19:00:00',
    'Team Alpha',
    ARRAY['Team Alpha', 'Team Beta'],
    $$Game Started at 2024-06-01 19:00:00 GMT
Red: "player1" as "Team Alpha"
Blue: "player2" as "Team Beta"
[00:00:05.00] ••• Team Alpha (player1) starts turn
[00:00:08.00] ••• Team Alpha (player1) fires Shotgun
[00:00:25.00] ••• Damage dealt: 45 to Team Beta (player2)
[00:00:27.00] ••• Team Alpha (player1) ends turn; time used: 22.00 sec turn, 3.00 sec retreat
[00:00:35.00] ••• Team Beta (player2) starts turn
[00:00:40.00] ••• Team Beta (player2) fires Grenade (3 sec, min bounce)
[00:00:58.00] ••• Damage dealt: 30 to Team Alpha (player1)
[00:01:00.00] ••• Team Beta (player2) ends turn; time used: 25.00 sec turn, 3.00 sec retreat
[00:01:10.00] ••• Team Alpha (player1) starts turn
[00:01:15.00] ••• Team Alpha (player1) fires Ninja Rope
[00:01:20.00] ••• Team Alpha (player1) fires Banana Bomb (5 sec)
[00:01:35.00] ••• Damage dealt: 80 (1 kill) to Team Beta (player2)
[00:01:37.00] ••• Team Alpha (player1) ends turn; time used: 27.00 sec turn, 3.00 sec retreat
[00:01:45.00] ••• Team Beta (player2) starts turn
[00:01:50.00] ••• Team Beta (player2) fires Bazooka
[00:02:05.00] ••• Damage dealt: 50 to Team Alpha (player1)
[00:02:07.00] ••• Team Beta (player2) ends turn; time used: 22.00 sec turn, 3.00 sec retreat
Team Alpha wins the match!
$$
);
```

The PostgreSQL column name is `fulllog` (all lowercase) per `V0.2.2__AddFullLogToReplayTable.sql` (`ALTER TABLE public.replays ADD COLUMN fulllog text`). Dapper maps it to the C# property via the `fullLog` alias in SELECT queries. In the INSERT statement, use the lowercase column name `fulllog`.

### 7. `Worms.Armageddon.Files` project reference in gateway

`src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj` already contains `<ProjectReference Include="..\Worms.Armageddon.Files\Worms.Armageddon.Files.csproj"/>`. No `.csproj` change is needed.

### 8. MUI icon imports

The sidebar uses `TimelineIcon` and `GpsFixedIcon`. These come from `@mui/icons-material` which is already a dependency. Import them as:

```tsx
import TimelineIcon from '@mui/icons-material/Timeline'
import GpsFixedIcon from '@mui/icons-material/GpsFixed'
```

Also needed in this page: `Box`, `Breadcrumbs`, `Chip`, `CircularProgress`, `Container`, `List`, `ListItemButton`, `ListItemIcon`, `ListItemText`, `MuiLink` (from `@mui/material/Link`), `Paper`, `Stack`, `Table`, `TableBody`, `TableCell`, `TableContainer`, `TableHead`, `TableRow`, `Typography`, `ListItemText`. Import each from its own path (`@mui/material/Box`, etc.) — consistent with the pattern in `LeagueDetailPage.tsx`.

---

## Verification

1. `dotnet build src/Worms.Hub.Gateway --warnaserror` — must produce no warnings or errors.
2. `make web.build && make web.lint` — must pass with no TypeScript errors, ESLint errors, or Prettier diffs.
3. `docker compose up` — seed data applies; navigate to the Redgate league page, click the seeded replay row with `full_log`; the detail page loads with a populated hero card (title "Match #006", correct date, winner chip "Team Alpha", two team chips, no scheme version chip since "Scheme v..." depends on league having a version), a stats strip (duration ~2m02s, 4 turns, max damage 80+50=130 or per-turn max = turn 3 with 80, kills = 1), Turn-by-turn panel showing 4 rows, and Weapons panel showing per-team breakdown.
4. Navigate to `/leagues/redgate/replays/999` — must show the not-found message.
5. Navigate to `/leagues/redgate/replays/{id-of-pending-replay}` — must show the "processing" message with no hero card or panels.
6. Open a browser DevTools network tab; confirm `GET /api/v1/leagues/redgate/replays/{id}` returns 200 with `turns` array present for the seeded replay.
7. Open the page without signing in — confirm the existing `RequireAuth` wrapper redirects to `/`.
8. Call `GET /api/v1/leagues/redgate/replays/{id}` with no `Authorization` header — must return 401.
