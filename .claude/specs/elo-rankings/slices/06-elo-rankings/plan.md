# Plan: ELO Rankings

## Context

This slice integrates the PlayerRank library to compute per-league ELO ratings from stored replay placement data, wires the calculation into the Worker's replay processing pipeline, and exposes the resulting standings on the existing league API endpoints and the `LeagueDetailPage` in the Web UI.

It builds on slices 01–05, which delivered: replay placement extraction and persistence (finish positions stored per replay in `replay_placements`); the `players` and `teams` tables with player alias claiming (V0.7 migration); and the `IFeatureFlags`/`GatewayFeatureFlags` pattern for schema-version gating. Nothing added by this slice overlaps with the deferred "ELO on alias changes" slice (slice 07), which will wire ELO recalculation to claim/unclaim actions.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/database/migrations/V0.8__AddPlayerRatings.sql` | Flyway migration creating the `player_ratings` table |
| `src/Worms.Hub.Storage/Domain/PlayerRating.cs` | Domain record for a per-player per-league ELO rating |
| `src/Worms.Hub.Storage/Database/IRatingsRepository.cs` | Interface: read all ratings for a league; replace all ratings for a league |
| `src/Worms.Hub.Storage/Database/RatingsRepository.cs` | Dapper/Npgsql implementation of `IRatingsRepository` |
| `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs` | Service that reads placements, runs PlayerRank, and writes ratings |
| `src/Worms.Hub.Gateway/API/DTOs/StandingDto.cs` | DTO record for one entry in the standings array |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj` | Add `PlayerRank` NuGet package reference (version 5.0.38) |
| `src/Worms.Hub.Storage/ServiceRegistration.cs` | Register `IRatingsRepository` as `Scoped` |
| `src/Worms.Hub.Gateway/FeatureFlags/IFeatureFlags.cs` | Add `IsEloRatingsEnabledAsync()` method |
| `src/Worms.Hub.Gateway/FeatureFlags/FeatureFlags.cs` | Implement `IsEloRatingsEnabledAsync()` — gate on V0.8 schema version |
| `src/Worms.Hub.Gateway/API/DTOs/LeagueDto.cs` | Add `Standings` field (`IReadOnlyList<StandingDto>?`); extend `FromDomain` |
| `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` | Inject `IRatingsRepository` + `IFeatureFlags`; populate `standings` on both `GetAll` and `Get` |
| `src/Worms.Hub.Gateway/ServiceRegistration.cs` | Register `RatingsCalculator` as `Scoped` in `AddWorkerServices()` |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Inject `RatingsCalculator`; call it after marking the replay `Processed`; catch + log exceptions |
| `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` | Add `StandingDto` interface and `standings` field to `LeagueDto`; render standings table above replays |

---

## Implementation Details

### 1. DB migration — `V0.8__AddPlayerRatings.sql`

Create `src/database/migrations/V0.8__AddPlayerRatings.sql`:

```sql
CREATE TABLE IF NOT EXISTS public.player_ratings (
    player_auth_subject  text    NOT NULL REFERENCES public.players (auth_subject),
    league_id            text    NOT NULL,
    rating               integer NOT NULL,
    games_played         integer NOT NULL,
    PRIMARY KEY (player_auth_subject, league_id)
);
```

The `league_id` is not a FK because the leagues table is seeded from config, not managed as a typical relational entity.

### 2. Domain model — `PlayerRating.cs`

Create `src/Worms.Hub.Storage/Domain/PlayerRating.cs`:

```csharp
using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record PlayerRating(
    string PlayerAuthSubject,
    string DisplayName,
    string LeagueId,
    int Rating,
    int GamesPlayed);
```

### 3. Storage — `IRatingsRepository` and `RatingsRepository`

`src/Worms.Hub.Storage/Database/IRatingsRepository.cs`:

```csharp
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IRatingsRepository
{
    IReadOnlyList<PlayerRating> GetByLeagueId(string leagueId);
    void ReplaceForLeague(string leagueId, IReadOnlyList<PlayerRating> ratings);
}
```

`src/Worms.Hub.Storage/Database/RatingsRepository.cs`:

```csharp
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public sealed class RatingsRepository(IConfiguration configuration) : IRatingsRepository
{
    public IReadOnlyList<PlayerRating> GetByLeagueId(string leagueId)
    {
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        return [.. connection.Query<RatingsDb>(
            "SELECT pr.player_auth_subject AS PlayerAuthSubject, p.display_name AS DisplayName, "
            + "pr.league_id AS LeagueId, pr.rating AS Rating, pr.games_played AS GamesPlayed "
            + "FROM player_ratings pr "
            + "JOIN players p ON p.auth_subject = pr.player_auth_subject "
            + "WHERE pr.league_id = @leagueId "
            + "ORDER BY pr.rating DESC",
            new { leagueId }).Select(r => new PlayerRating(r.PlayerAuthSubject, r.DisplayName, r.LeagueId, r.Rating, r.GamesPlayed))];
    }

    public void ReplaceForLeague(string leagueId, IReadOnlyList<PlayerRating> ratings)
    {
        ArgumentNullException.ThrowIfNull(ratings);
        var connectionString = configuration.GetConnectionString("Database");
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        _ = connection.Execute(
            "DELETE FROM player_ratings WHERE league_id = @leagueId",
            new { leagueId },
            transaction);
        foreach (var r in ratings)
        {
            _ = connection.Execute(
                "INSERT INTO player_ratings (player_auth_subject, league_id, rating, games_played) "
                + "VALUES (@playerAuthSubject, @leagueId, @rating, @gamesPlayed)",
                new { playerAuthSubject = r.PlayerAuthSubject, leagueId = r.LeagueId, rating = r.Rating, gamesPlayed = r.GamesPlayed },
                transaction);
        }
        transaction.Commit();
    }
}

[PublicAPI]
internal sealed record RatingsDb(
    string PlayerAuthSubject,
    string DisplayName,
    string LeagueId,
    int Rating,
    int GamesPlayed);
```

Note: `RatingsDb` is `internal sealed` because it is only used within the Storage assembly for Dapper mapping. The repository itself is `public sealed` because it is injected by interface from the Gateway assembly.

Register in `src/Worms.Hub.Storage/ServiceRegistration.cs`:

```csharp
.AddScoped<IRatingsRepository, RatingsRepository>()
```

### 4. PlayerRank NuGet dependency

Add to `src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj`:

```xml
<PackageReference Include="PlayerRank" Version="5.0.38"/>
```

### 5. `RatingsCalculator` service

Create `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs`.

The calculator:
- Accepts a `leagueId`.
- Reads all replays for the league ordered `date ASC, name ASC` (nulls last on date).
- Builds a lookup from `(machine, teamName)` to `playerAuthSubject` using `ITeamsRepository.GetAll()` filtered to claimed teams.
- Iterates replays; for each replay:
  - Identifies which placements are claimed (have a matching entry in the teams lookup).
  - Counts towards `gamesPlayed` for any player who has at least one claimed placement in the replay.
  - If fewer than 2 distinct claimed players, skips the ELO update but still counts games_played.
  - If 2+ distinct claimed players, adds a `Game` to the `League` for those players.
- Calls `league.GetLeaderBoard(new EloScoringStrategy(new Points(64), new Points(400), new Points(1000)))`.
- Merges `gamesPlayed` counts into the results (the leaderboard only tracks ELO, not games played).
- Writes results via `IRatingsRepository.ReplaceForLeague`.

`EloScoringStrategy` parameters:
- `maxRatingChange = new Points(64)` — standard K-factor
- `maxSkillGap = new Points(400)` — standard ELO denominator scaling
- `startingRating = new Points(1000)` — as required by the spec

Reading ELO values: `(int)playerScore.Points.GetValue()` — the library rounds every delta with `MidpointRounding.AwayFromZero` so the value is always a whole number; casting via `(int)` is safe.

```csharp
using System.Diagnostics.CodeAnalysis;
using PlayerRank;
using PlayerRank.Scoring.Elo;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Ratings;

internal sealed class RatingsCalculator(
    IReplaysRepository replaysRepository,
    ITeamsRepository teamsRepository,
    IRatingsRepository ratingsRepository)
{
    public void Calculate(string leagueId)
    {
        // Build alias lookup: (machine, teamName) -> playerAuthSubject
        var claimedTeams = teamsRepository.GetAll()
            .Where(t => t.ClaimedByAuthSubject is not null)
            .ToDictionary(t => (t.Machine, t.TeamName), t => t.ClaimedByAuthSubject!);

        // Replays ordered by date ASC, then name ASC as tiebreaker (nulls last)
        var replays = replaysRepository.GetByLeagueId(leagueId)
            .Where(r => r.Status == "Processed" && r.Placements is { Count: > 0 })
            .OrderBy(r => r.Date ?? DateTime.MaxValue)
            .ThenBy(r => r.Name)
            .ToList();

        var league = new League();
        // games_played counts: playerAuthSubject -> count of replays with at least one claimed placement
        var gamesPlayed = new Dictionary<string, int>();

        foreach (var replay in replays)
        {
            // Map placements to claimed players (may have duplicates if same player has multiple teams in one game — take first)
            var matchedPlayers = replay.Placements!
                .Where(p => p.Position.HasValue && claimedTeams.ContainsKey((p.Machine, p.TeamName)))
                .Select(p => (AuthSubject: claimedTeams[(p.Machine, p.TeamName)], Position: p.Position!.Value))
                .GroupBy(x => x.AuthSubject)
                .Select(g => g.OrderBy(x => x.Position).First()) // best position if same player has multiple teams
                .ToList();

            // Count games_played for each matched player
            foreach (var mp in matchedPlayers)
            {
                gamesPlayed.TryAdd(mp.AuthSubject, 0);
                gamesPlayed[mp.AuthSubject]++;
            }

            // ELO: only include replays with 2+ distinct matched players
            if (matchedPlayers.Count < 2)
            {
                continue;
            }

            var game = new Game();
            foreach (var mp in matchedPlayers)
            {
                game.AddResult(mp.AuthSubject, new Position(mp.Position));
            }
            league.RecordGame(game);
        }

        var eloStrategy = new EloScoringStrategy(new Points(64), new Points(400), new Points(1000));
        var leaderboard = league.GetLeaderBoard(eloStrategy).ToList();

        // Build result: all players with at least one game
        var ratings = gamesPlayed.Keys.Select(authSubject =>
        {
            var score = leaderboard.FirstOrDefault(s => s.Name == authSubject);
            var elo = score is not null ? (int)score.Points.GetValue() : 1000;
            return new PlayerRating(authSubject, string.Empty, leagueId, elo, gamesPlayed[authSubject]);
        }).ToList();

        ratingsRepository.ReplaceForLeague(leagueId, ratings);
    }
}
```

**Note on `DisplayName` in ratings**: `RatingsRepository.ReplaceForLeague` does not insert display names (they live in the `players` table and are joined on read). The `PlayerRating` domain record passed to `ReplaceForLeague` has `DisplayName = string.Empty` — this is only a transport value; the persisted `player_ratings` row does not store display name.

Register in `AddWorkerServices()` in `src/Worms.Hub.Gateway/ServiceRegistration.cs`:

```csharp
.AddScoped<RatingsCalculator>()
```

The gateway also needs ratings for the API (read side), so `IRatingsRepository` is already available via `AddHubStorageServices()` which is called from `AddWorkerServices()`. For the gateway mode, `AddGatewayServices()` does not call `AddHubStorageServices()` — instead, `Program.cs` calls them independently. Verify that `AddHubStorageServices()` is called when running in gateway mode so `IRatingsRepository` is available to `LeaguesController`. Looking at `Program.cs` to confirm (see verification step below). If `AddHubStorageServices()` is already wired for the gateway path, no change is needed there.

### 6. Wire `RatingsCalculator` into `Processor`

In `src/Worms.Hub.Gateway/Worker/Processor.cs`:

Add `RatingsCalculator ratingsCalculator` to the constructor parameter list.

After the `replayRepository.Update(updatedReplay)` call and the teams upsert block, and before the `AnnounceGameComplete` call, add:

```csharp
// Calculate ELO ratings for the league
if (await featureFlags.IsEloRatingsEnabledAsync() && updatedReplay.LeagueId is not null)
{
    try
    {
        ratingsCalculator.Calculate(updatedReplay.LeagueId);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", updatedReplay.LeagueId);
    }
}
```

Add `[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "ELO calculation failure must not block replay processing")]` to the `UpdateReplay` method, consistent with how `PlacementsBackfiller.RunAsync` suppresses CA1031.

### 7. Feature flags

`src/Worms.Hub.Gateway/FeatureFlags/IFeatureFlags.cs` — add method:

```csharp
Task<bool> IsEloRatingsEnabledAsync();
```

`src/Worms.Hub.Gateway/FeatureFlags/FeatureFlags.cs` — add field and implementation:

```csharp
private static readonly Version EloRatingsMinVersion = new(0, 8);

public async Task<bool> IsEloRatingsEnabledAsync()
{
    var current = await schemaVersion.GetCurrentVersionAsync();
    return current is not null && current >= EloRatingsMinVersion;
}
```

### 8. `LeagueDto` and `StandingDto`

Create `src/Worms.Hub.Gateway/API/DTOs/StandingDto.cs`:

```csharp
using JetBrains.Annotations;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record StandingDto(string PlayerName, int Elo, int GamesPlayed);
```

Modify `src/Worms.Hub.Gateway/API/DTOs/LeagueDto.cs`:

```csharp
using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.API.DTOs;

[PublicAPI]
internal sealed record LeagueDto(string Id, string Name, Version? Version, Uri? SchemeUrl, IReadOnlyList<StandingDto>? Standings)
{
    internal static LeagueDto FromDomain(
        string id,
        string name,
        League? league,
        Uri schemeUrl,
        IReadOnlyList<StandingDto>? standings) =>
        league is null
            ? new(id, name, null, null, standings)
            : new(id, name, league.Version, schemeUrl, standings);
}
```

The `standings` parameter is `null` when ELO ratings are not yet enabled (migration not applied), and an empty list when enabled but no rated players exist.

### 9. `LeaguesController` changes

Inject `IRatingsRepository ratingsRepository` and `IFeatureFlags featureFlags` into the constructor. Note: `IFeatureFlags` is already registered via `AddGatewayServices()` — no additional registration needed. `IRatingsRepository` is registered via `AddHubStorageServices()`.

Verify `Program.cs` calls `AddHubStorageServices()` for the gateway mode. If it does not, add it or add `IRatingsRepository` registration to `AddGatewayServices()`.

In both `GetAll()` and `Get()`, before constructing `LeagueDto`:

```csharp
IReadOnlyList<StandingDto>? standings = null;
if (await featureFlags.IsEloRatingsEnabledAsync())
{
    // leagueId is the relevant league's id
    standings = ratingsRepository.GetByLeagueId(leagueId)
        .Select(r => new StandingDto(r.DisplayName, r.Rating, r.GamesPlayed))
        .ToList();
}
```

Pass `standings` to `LeagueDto.FromDomain(...)`.

For `GetAll()`, iterate each league and fetch its standings inside the existing `tasks` LINQ select.

**Scope decision — list vs detail asymmetry**: Both `GET /api/v1/leagues` (list) and `GET /api/v1/leagues/{id}` (detail) return `standings`. There is no asymmetry — both are updated. The list endpoint fetches standings for each league individually (one DB query per league); this is acceptable given the small number of active leagues.

### 10. Verify `Program.cs` wiring

Before implementing the controller change, read `src/Worms.Hub.Gateway/Program.cs` to confirm that `AddHubStorageServices()` (which registers `IRatingsRepository`) is called when `runGateway = true`. If it is not, add `.AddHubStorageServices()` to the gateway path. This is a required verification step — do not skip it.

### 11. Web UI — `LeagueDetailPage.tsx`

Add `StandingDto` interface and extend `LeagueDto`:

```typescript
interface StandingDto {
    playerName: string
    elo: number
    gamesPlayed: number
}

interface LeagueDto {
    id: string
    name: string
    version: string | null
    schemeUrl: string | null
    standings: StandingDto[] | null
}
```

The `useEffect` already fetches `GET /api/v1/leagues/${id}` — no change to the fetch logic needed.

Add the standings table above the replays table (inside the `{league !== null && replays !== null && (` block, before the replays section). Render only when `league.standings` is non-null and `league.standings.length > 0`:

```tsx
{league.standings !== null && league.standings.length > 0 && (
    <Box sx={{ mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 700, mb: 1.5 }}>
            Standings
        </Typography>
        <TableContainer component={Paper} variant="outlined">
            <Table size="small">
                <TableHead>
                    <TableRow>
                        <TableCell sx={{ fontWeight: 700, width: 60 }}>Rank</TableCell>
                        <TableCell sx={{ fontWeight: 700 }}>Player Name</TableCell>
                        <TableCell sx={{ fontWeight: 700, width: 100 }} align="right">ELO</TableCell>
                        <TableCell sx={{ fontWeight: 700, width: 120 }} align="right">Games Played</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {league.standings.map((s, index) => (
                        <TableRow key={s.playerName}>
                            <TableCell>
                                <Typography sx={{ fontFamily: monoFontFamily, fontSize: 12, color: 'text.secondary' }}>
                                    {index + 1}
                                </Typography>
                            </TableCell>
                            <TableCell>
                                <Typography variant="body2">{s.playerName}</Typography>
                            </TableCell>
                            <TableCell align="right">
                                <Typography sx={{ fontFamily: monoFontFamily, fontSize: 14, fontWeight: 700 }}>
                                    {s.elo}
                                </Typography>
                            </TableCell>
                            <TableCell align="right">
                                <Typography variant="body2" color="text.secondary">
                                    {s.gamesPlayed}
                                </Typography>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    </Box>
)}
```

`LeagueListPage.tsx` does not need changes — `standings` added to the DTO will be silently ignored by the existing page (it does not render standings; that is deferred to later slices).

Run `npx prettier --write src` inside `src/Worms.Hub.Web/` after editing, then `npm ci && make web.lint` to confirm no lint errors.

---

## Caveats and known risks

- **`Position` ordering in PlayerRank**: `Position(1)` is first place (highest). In the EloScoringStrategy, `PlayerAWon` returns `true` when `playerAResult.Position > playerBResult.Position`, and `Position.operator>` returns true when `_position < other._position` (lower integer = better). So `new Position(1)` correctly beats `new Position(2)`. Pass the numeric position from `ReplayPlacement.Position` directly as `new Position(mp.Position)`.

- **Null positions in placements**: Placements can have `Position = null` (V0.6 made position nullable). Filter with `.Where(p => p.Position.HasValue)` before mapping to PlayerRank.

- **`[SuppressMessage]` placement**: Per learnings from slice 02, `[SuppressMessage]` for CA1031 must be placed on the enclosing method (`UpdateReplay`), not on the `catch` clause.

- **`TryAddScoped` namespace**: Per learnings from slice 02, `TryAddScoped` requires `using Microsoft.Extensions.DependencyInjection.Extensions;` which is not in implicit usings. `AddScoped` (used for `RatingsCalculator`) does not have this issue.

- **Roslynator RCS1124**: Inline single-use local variables. Where a variable is only used in one following statement, inline it directly to avoid the RCS1124 warning-as-error (e.g., inline the `leaderboard` list rather than assigning then immediately returning).

- **`internal sealed record`**: All new `record` types that are not consumed from outside their assembly must be `internal sealed` to avoid CA1852.

- **`useCallback` pattern in React**: Do not introduce `useCallback` in the web changes. The existing `useEffect` fetch pattern (with `.then()` chains, no `async/await` in the effect body) must be preserved. No mutation buttons are added in this slice, so no `refetchKey` pattern is needed.

- **`npm ci` before `make web.lint`**: Per learnings from slice 05, run `npm ci` inside `src/Worms.Hub.Web/` before `make web.lint`.

---

## Verification

1. `dotnet build src/Worms.Hub.Gateway --warnaserror` — confirms PlayerRank dependency resolves, all C# compiles clean with no warnings.
2. `docker compose up -d` then verify Flyway applies V0.8 migration cleanly by checking `SELECT * FROM flyway_schema_history ORDER BY installed_rank DESC LIMIT 3`.
3. Before migration applied: `GET /api/v1/leagues` and `GET /api/v1/leagues/{id}` return `"standings": null` and HTTP 200.
4. After migration applied and at least one replay processed with a claimed alias: `GET /api/v1/leagues/{id}` returns `"standings": [{"playerName": "...", "elo": ..., "gamesPlayed": ...}]` ordered by ELO descending.
5. League with no claimed aliases: `GET /api/v1/leagues/{id}` returns `"standings": []`.
6. Simulate `RatingsCalculator` throwing (e.g., disconnect DB mid-run): replay is still marked `Processed`, queue message deleted, error appears in logs.
7. `npm ci && make web.lint` inside `src/Worms.Hub.Web/` passes with no ESLint, TypeScript, or Prettier errors.
8. Load the league detail page in the browser for a league with rated players: standings table appears above the replays table with Rank, Player Name, ELO, and Games Played columns.
9. Load the league detail page for a league with no rated players: no standings section is rendered.
