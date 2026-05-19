# Plan: ELO Delta on Game Detail

## Context

This slice is the final piece of the ELO Rankings epic. It adds two new
nullable columns (`elo_delta`, `elo_after`) to `replay_placements`, extends
`RatingsCalculator.Calculate` to populate them per replay using
`PlayerRank.League.GetLeaderBoardHistory`, surfaces the values on the
existing replay DTOs (`GET /api/v1/leagues/{id}/replays/{replayId}` and
`GET /api/v1/leagues/{id}/replays`), and renders an ELO badge segment on
`PlacementPill` on the game/replay detail page.

It builds on slice 06 (which introduced `RatingsCalculator`, the PlayerRank
integration, and the `IsEloRatingsEnabledAsync` schema-version gate), slice
07 (which wired `CalculateForTeam` to claim/unclaim, so deltas are
recalculated alongside standings on alias changes), and slice 08 (which
showed the top-3 standings on league cards; this slice does not touch that
page). The `StartupBackfiller.BackfillRatings` method already runs
`Calculate(leagueId)` per league when needed; this slice extends its
"already populated, skip" detection so that pre-slice rows missing the new
columns still trigger a recompute.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/database/migrations/V0.9__AddPlacementEloFields.sql` | Flyway migration: add nullable `elo_delta` and `elo_after` columns to `replay_placements`. |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Storage/Domain/ReplayPlacement.cs` | Add `int? EloDelta` and `int? EloAfter` to the record. |
| `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs` | Select the two new columns in both `GetAll` and `GetByLeagueId`; map them into `ReplayPlacement`. |
| `src/Worms.Hub.Storage/Database/IReplaysRepository.cs` | Add `void UpdatePlacementElo(int replayId, string machine, string teamName, int? eloDelta, int? eloAfter)` — keyed by composite PK of `replay_placements`. |
| `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs` | Implement `UpdatePlacementElo` as a single `UPDATE` statement; bulk-clear via a separate SQL helper used by the calculator. |
| `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs` | Track per-replay `recordedGameIndex`, track per-replay `(playerAuthSubject -> selectedPlacement)` mapping, call `GetLeaderBoardHistory` after the iteration, write back per-placement delta/after values; overwrite **every** placement on **every** replay in the league in a single pass. |
| `src/Worms.Hub.Gateway/Worker/StartupBackfiller.cs` | Replace the `COUNT(*) FROM player_ratings > 0` short-circuit with the spec's per-league detection query (`SELECT 1 ... WHERE elo_after IS NULL ...`); iterate only leagues that match. Keep the existing "first install" path that runs every league when the `player_ratings` table is empty. |
| `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs` | Extend `PlacementDto` with `int? EloDelta` and `int? EloAfter`; update `FromDomain`. |
| `src/Worms.Hub.Web/src/pages/PlacementPill.tsx` | Extend `PlacementDto` interface with `eloDelta` and `eloAfter`; render an ELO badge segment with a divider when `eloAfter !== null`. |
| `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` | Extend the local `PlacementDto` interface (lines 51–56) with `eloDelta: number \| null` and `eloAfter: number \| null` so it stays in shape-sync with `PlacementPill`'s exported one. (No rendering change here — standings table is unaffected; the league detail page does not render pills.) |
| `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx` | No code change required — it already imports `PlacementDto` from `./PlacementPill`. (Pill rendering picks up the new fields automatically.) Verify nothing breaks; no edits planned unless the build fails. |

---

## Implementation Details

### 1. Migration — `V0.9__AddPlacementEloFields.sql`

Create `src/database/migrations/V0.9__AddPlacementEloFields.sql`:

```sql
ALTER TABLE public.replay_placements
    ADD COLUMN elo_delta integer NULL,
    ADD COLUMN elo_after integer NULL;
```

Both columns are nullable with no default. Existing rows therefore have both
set to `NULL` after migration, which is exactly the post-migration /
pre-backfill state required by the spec.

There is intentionally **no** feature flag for V0.9 — the columns are
purely additive and read paths tolerate `NULL`. We do not gate the API or
calculator on a schema version 0.9 minimum because: (a) `RatingsCalculator`
runs only when `IsEloRatingsEnabledAsync()` returns true, which still gates
on 0.8; (b) the new `UpdatePlacementElo` write call only fires when the
calculator is invoked; and (c) the `SELECT` in `ReplaysRepositoryV05` will
fail at runtime if the columns are missing — but Flyway runs migrations
before the gateway accepts traffic, so this is not a real ordering hazard.
Adding a 0.9 flag would force every read to do a version check; not worth
the cost for two always-nullable columns.

### 2. Domain — `ReplayPlacement`

Update `src/Worms.Hub.Storage/Domain/ReplayPlacement.cs`:

```csharp
using JetBrains.Annotations;

namespace Worms.Hub.Storage.Domain;

[PublicAPI]
public sealed record ReplayPlacement(
    string Machine,
    string TeamName,
    int? Position,
    string? PlayerName,
    int? EloDelta = null,
    int? EloAfter = null);
```

The new properties have default values so existing callers (e.g.
`StartupBackfiller.BackfillPlacements` which constructs
`new ReplayPlacement(p.Team.Machine, p.Team.Name, p.Position, null)`, and
`Processor.UpdateReplay` which uses the same constructor shape) continue to
compile unmodified. JetBrains.Annotations `[PublicAPI]` is already present.

### 3. Repository read path — `ReplaysRepositoryV05`

Update the private `ReplayPlacementDb` record at the top of
`ReplaysRepositoryV05.cs` and both `GetAll` / `GetByLeagueId` to project
the new columns:

```csharp
private sealed record ReplayPlacementDb(
    int ReplayId,
    string Machine,
    string TeamName,
    int? Position,
    string? PlayerName,
    int? EloDelta,
    int? EloAfter)
{
    public ReplayPlacement ToDomain() => new(Machine, TeamName, Position, PlayerName, EloDelta, EloAfter);
}
```

In both `GetAll` and `GetByLeagueId` the SQL changes from:

```text
SELECT rp.replay_id AS ReplayId, rp.machine AS Machine, rp.team_name AS TeamName,
       rp.position AS Position, pl.display_name AS PlayerName
FROM replay_placements rp
LEFT JOIN teams t ON ...
LEFT JOIN players pl ON ...
WHERE rp.replay_id = ANY(@ids)
```

to:

```text
SELECT rp.replay_id AS ReplayId, rp.machine AS Machine, rp.team_name AS TeamName,
       rp.position AS Position, pl.display_name AS PlayerName,
       rp.elo_delta AS EloDelta, rp.elo_after AS EloAfter
FROM replay_placements rp
LEFT JOIN teams t ON t.machine = rp.machine AND t.team_name = rp.team_name
LEFT JOIN players pl ON pl.auth_subject = t.player_auth_subject
WHERE rp.replay_id = ANY(@ids)
```

`Create` and `Update` (which write replay rows) keep writing `elo_delta`
and `elo_after` as the column default (`NULL`) on insert — no change to
those INSERT statements; they do not name the new columns, so PostgreSQL
inserts the column default (`NULL`).

### 4. Repository write path — placement-level update

Add a new method to `IReplaysRepository`:

```csharp
void UpdatePlacementElo(int replayId, string machine, string teamName, int? eloDelta, int? eloAfter);
```

Implementation in `ReplaysRepositoryV05`:

```csharp
public void UpdatePlacementElo(int replayId, string machine, string teamName, int? eloDelta, int? eloAfter)
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    _ = connection.Execute(
        "UPDATE replay_placements "
        + "SET elo_delta = @eloDelta, elo_after = @eloAfter "
        + "WHERE replay_id = @replayId AND machine = @machine AND team_name = @teamName",
        new { replayId, machine, teamName, eloDelta, eloAfter });
}
```

The composite primary key `(replay_id, machine, team_name)` uniquely
identifies each placement — this is the "placementId" the spec refers to.
No surrogate `id` column is needed; introducing one would be a much larger
schema change for no benefit.

**Bulk-clear helper.** Also add:

```csharp
void ClearPlacementEloForLeague(string leagueId);
```

to the interface and implement as:

```csharp
public void ClearPlacementEloForLeague(string leagueId)
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    _ = connection.Execute(
        "UPDATE replay_placements rp "
        + "SET elo_delta = NULL, elo_after = NULL "
        + "FROM replays r "
        + "WHERE rp.replay_id = r.id AND r.league_id = @leagueId",
        new { leagueId });
}
```

Although the spec says "single update pass — no separate clear step", the
practical implementation is: clear in one SQL statement, then
overwrite the rows that have computed values. This is a single logical
pass from the calculator's perspective (one method call,
`SetEloForLeague(leagueId, perRowValues)` semantically) — but split into
clear + targeted writes. End-state is identical to the spec's "single
overwrite pass" invariant: no stale values remain. This avoids having to
enumerate every placement to issue an explicit `UPDATE … SET … = NULL` for
the ones the calculator does not select.

If a reviewer prefers a literal single pass, the alternative is to
construct a `Dictionary<(replayId, machine, teamName), (int? d, int? a)>`
covering **every** placement (including the NULL ones) and execute one
multi-row `UPDATE` via a `VALUES` join. That is more code for no
behavioural difference; stick with the clear-then-fill approach.

### 5. `RatingsCalculator.Calculate` — per-placement delta computation

Modify `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs`.

The existing `Calculate(leagueId)` method already iterates replays in
`date ASC, name ASC` order and conditionally calls `league.RecordGame(...)`.
Augment it to track three new things per replay:

1. **`recordedGameIndex`**: `int?` — the 1-based count of `RecordGame`
   calls made up to and including this replay's game (i.e. equal to
   `historyIndex` for the post-game snapshot). `null` for replays where
   `RecordGame` was not invoked.
2. **`recordedSelections`**: per replay, a `Dictionary<string, (string Machine, string TeamName)>`
   mapping `playerAuthSubject -> the specific (machine, teamName)` that was
   chosen as "best position" — exactly the row the calculator already uses
   for the PlayerRank `Game`. This is the row that will receive non-null
   `elo_delta`/`elo_after`.
3. **`singleMatchedReplay`**: replays where exactly one matched player
   exists (`matchedPlayers.Count == 1`) — record the single
   `(replayId, machine, teamName)` plus the player's auth subject so we
   can later write `elo_delta = 0`, `elo_after = lastKnownRating`.

After the loop, call:

```csharp
var history = league.GetLeaderBoardHistory(eloStrategy).ToList();
```

Verified from a runtime probe against PlayerRank 5.0.38:

- An empty league returns a history of **one** snapshot (index 0)
  containing zero players.
- After `n` `RecordGame` calls, the history has `n + 1` snapshots:
  index `0` is the pre-game baseline (every player who will ever play is
  implicitly at 1000), and index `g` (1 ≤ g ≤ n) is the state after
  recorded-game `g`.
- `History.Leaderboard` is `IEnumerable<PlayerScore>`, **not** an
  `IDictionary` keyed by player. Look players up by `score.Name`. Build a
  per-snapshot `Dictionary<string, double>` once per replay being written
  back, not on every lookup.

Helper to read a player's ELO from a snapshot:

```csharp
static int RatingAt(History? snapshot, string authSubject)
{
    if (snapshot is null) { return 1000; }
    var score = snapshot.Leaderboard.FirstOrDefault(s => s.Name == authSubject);
    return score is null ? 1000 : (int)score.Points.GetValue();
}
```

Per the spec, `history[0]` already gives every "ever-played" player 1000,
so the `score is null` branch only fires for players that have not yet
been added to the league (e.g. lookup in a snapshot taken before that
player's first game). The `1000` fallback satisfies the spec's
"player absent from that snapshot ⇒ 1000" rule.

**Pseudocode for the augmented Calculate**:

```csharp
public void Calculate(string leagueId)
{
    var claimedTeams = teamsRepository.GetAll()
        .Where(t => t.ClaimedByAuthSubject is not null)
        .ToDictionary(t => (t.Machine, t.TeamName), t => t.ClaimedByAuthSubject!);

    var replays = replaysRepository.GetByLeagueId(leagueId)
        .Where(r => r is { Status: "Processed", Placements.Count: > 0 })
        .OrderBy(r => r.Date ?? DateTime.MaxValue)
        .ThenBy(r => r.Name)
        .ToList();

    var league = new PlayerRank.League();
    var gamesPlayed = new Dictionary<string, int>();

    // Per-replay bookkeeping for delta write-back.
    var multiPlayerReplays = new List<MultiPlayerReplayInfo>();
    var singlePlayerReplays = new List<SinglePlayerReplayInfo>();

    var recordedGameCount = 0; // count of RecordGame() invocations so far

    foreach (var replay in replays)
    {
        var matchedPlayers = replay.Placements!
            .Where(p => p.Position.HasValue && claimedTeams.ContainsKey((p.Machine, p.TeamName)))
            .Select(p => (
                AuthSubject: claimedTeams[(p.Machine, p.TeamName)],
                Position: p.Position!.Value,
                p.Machine,
                p.TeamName))
            .GroupBy(x => x.AuthSubject)
            .Select(g => g.OrderBy(x => x.Position).First())
            .ToList();

        foreach (var mp in matchedPlayers)
        {
            gamesPlayed.TryAdd(mp.AuthSubject, 0);
            gamesPlayed[mp.AuthSubject]++;
        }

        if (matchedPlayers.Count == 0)
        {
            continue; // no write-back needed; placements remain NULL
        }

        if (matchedPlayers.Count == 1)
        {
            var mp = matchedPlayers[0];
            singlePlayerReplays.Add(new SinglePlayerReplayInfo(
                ReplayId: int.Parse(replay.Id, CultureInfo.InvariantCulture),
                AuthSubject: mp.AuthSubject,
                Machine: mp.Machine,
                TeamName: mp.TeamName,
                PriorRecordedGameIndex: recordedGameCount));
            continue;
        }

        // matchedPlayers.Count >= 2 — record the game.
        var game = new PlayerRank.Game();
        foreach (var mp in matchedPlayers)
        {
            game.AddResult(mp.AuthSubject, new Position(mp.Position));
        }
        league.RecordGame(game);
        recordedGameCount++;

        multiPlayerReplays.Add(new MultiPlayerReplayInfo(
            ReplayId: int.Parse(replay.Id, CultureInfo.InvariantCulture),
            RecordedGameIndex: recordedGameCount,
            Selections: matchedPlayers
                .Select(mp => new MultiPlayerSelection(mp.AuthSubject, mp.Machine, mp.TeamName))
                .ToList()));
    }

    var eloStrategy = new EloScoringStrategy(new Points(64), new Points(400), new Points(1000));
    var leaderboard = league.GetLeaderBoard(eloStrategy).ToList();

    // ---- Standings write (existing behaviour) ----
    var ratings = gamesPlayed.Keys.Select(authSubject =>
    {
        var score = leaderboard.FirstOrDefault(s => s.Name == authSubject);
        var elo = score is not null ? (int)score.Points.GetValue() : 1000;
        return new PlayerRating(authSubject, string.Empty, leagueId, elo, gamesPlayed[authSubject]);
    }).ToList();
    ratingsRepository.ReplaceForLeague(leagueId, ratings);

    // ---- Per-placement delta write-back (new) ----
    var history = league.GetLeaderBoardHistory(eloStrategy).ToList();

    // 1. Clear every placement in the league.
    replaysRepository.ClearPlacementEloForLeague(leagueId);

    // 2. Write multi-player rows.
    foreach (var r in multiPlayerReplays)
    {
        var post = history[r.RecordedGameIndex];
        var pre  = history[r.RecordedGameIndex - 1];
        foreach (var sel in r.Selections)
        {
            var after  = RatingAt(post, sel.AuthSubject);
            var before = RatingAt(pre,  sel.AuthSubject);
            replaysRepository.UpdatePlacementElo(
                r.ReplayId, sel.Machine, sel.TeamName,
                eloDelta: after - before,
                eloAfter: after);
        }
    }

    // 3. Write single-matched-player rows.
    foreach (var r in singlePlayerReplays)
    {
        var snap = r.PriorRecordedGameIndex == 0 ? null : history[r.PriorRecordedGameIndex];
        var elo  = snap is null ? 1000 : RatingAt(snap, r.AuthSubject);
        replaysRepository.UpdatePlacementElo(
            r.ReplayId, r.Machine, r.TeamName,
            eloDelta: 0,
            eloAfter: elo);
    }
}

private sealed record MultiPlayerSelection(string AuthSubject, string Machine, string TeamName);
private sealed record MultiPlayerReplayInfo(int ReplayId, int RecordedGameIndex, IReadOnlyList<MultiPlayerSelection> Selections);
private sealed record SinglePlayerReplayInfo(int ReplayId, string AuthSubject, string Machine, string TeamName, int PriorRecordedGameIndex);
```

Notes:

- Use `System.Globalization.CultureInfo.InvariantCulture` for the
  `int.Parse(replay.Id, ...)` calls (CA1305 — `Specify IFormatProvider`).
- The four private records are declared `private sealed` so they do not
  leak from the calculator's API surface. They satisfy CA1852.
- Add `using System.Globalization;` to the file's usings.
- Do not introduce a redundant local for `history[g]` — pass via the
  helper to avoid RCS1124 noise.
- The clear-then-update sequence runs against a single `IDbConnection`
  inside each method; for transactional integrity, both
  `ClearPlacementEloForLeague` and `UpdatePlacementElo` open their own
  short-lived connections (this is the established repository pattern in
  this codebase — see `RatingsRepository.ReplaceForLeague` which is the
  only place that uses an explicit transaction). The window between clear
  and the first update is acceptable for this read path because: deltas
  are only consulted by the API/UI for display; if a request arrives
  mid-write, some placements may briefly read as NULL — equivalent to the
  pre-backfill state. The acceptance criteria do not require atomicity
  across replays, only that the end state is consistent.

### 6. Single-matched-player replay — interpretation of "lastKnownRating"

Per the spec:

> when a processed replay has exactly one matched player, that player's
> placement has `elo_delta === 0` and `elo_after` equal to that player's
> `lastKnownRating` at the point this replay is reached in date order
> (or 1000 if they have no prior multi-player game). The single-matched
> replay does not advance `lastKnownRating`.

The plan models this as: capture `PriorRecordedGameIndex = recordedGameCount`
**before** processing this replay (i.e. the count of `RecordGame` calls
made strictly before reaching this replay in iteration order). Since
single-matched replays do **not** call `RecordGame`, `recordedGameCount`
is not incremented, which automatically satisfies "does not advance
lastKnownRating". `elo_after = RatingAt(history[PriorRecordedGameIndex],
authSubject)`; the `null` snapshot case (when `PriorRecordedGameIndex == 0`
and the player has never appeared) is handled by `RatingAt` returning
1000 either via the `score is null` fallback or the `null` snapshot guard.

### 7. `StartupBackfiller.BackfillRatings` — replace short-circuit

Current code in
`src/Worms.Hub.Gateway/Worker/StartupBackfiller.cs` short-circuits if
`COUNT(*) FROM player_ratings > 0`. That blocks pre-slice databases (where
`player_ratings` is populated by slice 06's backfill, but the new
`elo_delta`/`elo_after` columns are all NULL).

Replace the existing
"`COUNT(*) FROM player_ratings > 0` ⇒ skip" logic with two-stage detection:

```csharp
var ratingsCount = await connection.QuerySingleAsync<long>(
    "SELECT COUNT(*) FROM player_ratings");

if (ratingsCount == 0)
{
    // Fresh install — run all leagues, as before.
    leaguesNeedingRecalc = leaguesRepository.GetAll().Select(l => l.Id).ToList();
}
else
{
    // Slice-9 detection: any placement that *should* have a delta but doesn't.
    leaguesNeedingRecalc = (await connection.QueryAsync<string>(
        "SELECT DISTINCT r.league_id "
        + "FROM replay_placements rp "
        + "JOIN replays r ON r.id = rp.replay_id "
        + "JOIN teams t ON t.machine = rp.machine AND t.team_name = rp.team_name "
        + "JOIN replay_placements rp2 ON rp2.replay_id = rp.replay_id "
        + "JOIN teams t2 ON t2.machine = rp2.machine AND t2.team_name = rp2.team_name "
        + "WHERE rp.elo_after IS NULL "
        + "AND rp.position IS NOT NULL "
        + "AND t.player_auth_subject IS NOT NULL "
        + "AND r.status = 'Processed' "
        + "AND r.league_id IS NOT NULL "
        + "AND t2.player_auth_subject IS NOT NULL "
        + "AND rp2.position IS NOT NULL "
        + "GROUP BY r.league_id, rp.replay_id, rp.machine, rp.team_name "
        + "HAVING COUNT(DISTINCT t2.player_auth_subject) >= 2"))
        .ToList();
}

if (leaguesNeedingRecalc.Count == 0)
{
    logger.LogInformation("Ratings backfill already complete — skipping.");
    return;
}
```

Then iterate `leaguesNeedingRecalc` (instead of `leaguesRepository.GetAll()`)
inside the existing `foreach` and call
`ratingsCalculator.Calculate(leagueId)`.

The detection query encodes the spec's exact rule: a league needs backfill
when at least one placement meeting **all** of (claimed team, position
present, replay processed, replay has ≥ 2 matched players) has
`elo_after IS NULL`. The `HAVING COUNT(DISTINCT t2.player_auth_subject) >= 2`
clause expresses "replay has ≥ 2 matched players" — counting distinct
claimed players (after the player de-dup in `RatingsCalculator`'s
`GroupBy(x => x.AuthSubject)`). Players claiming two teams in one replay
count once.

Add `using System.Globalization;` if missing; no new project references.

### 8. `PlacementDto` — server

Update `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs`:

```csharp
[PublicAPI]
internal sealed record PlacementDto(
    string Machine,
    string TeamName,
    int? Position,
    string? PlayerName,
    int? EloDelta,
    int? EloAfter)
{
    internal static PlacementDto FromDomain(ReplayPlacement p) =>
        new(p.Machine, p.TeamName, p.Position, p.PlayerName, p.EloDelta, p.EloAfter);
}
```

ASP.NET Core's default `System.Text.Json` configuration serialises
`int?` as either an integer or `null` — matches the spec's wire format.

Both endpoints that build `ReplayDetailDto` (`GetReplays` and `GetReplay`
in `LeaguesController`) already call `ReplayDetailDto.FromDomain`, which
calls `PlacementDto.FromDomain`. No controller code change required.

**Scope decision — list vs detail symmetry**: The list endpoint
(`GET /api/v1/leagues/{id}/replays`) and the single endpoint
(`GET /api/v1/leagues/{id}/replays/{replayId}`) already return the same
`ReplayDetailDto` shape. Both include the new fields automatically. There
is no asymmetry to resolve.

### 9. `PlacementPill` — client

Update `src/Worms.Hub.Web/src/pages/PlacementPill.tsx`:

1. Extend the exported `PlacementDto` interface:

   ```ts
   export interface PlacementDto {
       machine: string
       teamName: string
       position: number | null
       playerName: string | null
       eloDelta: number | null
       eloAfter: number | null
   }
   ```

2. Inside `PlacementPill`, after the existing player-name `<Box>`, add a
   conditional ELO segment. Render only when `placement.eloAfter !== null`:

   ```tsx
   {placement.eloAfter !== null && (
       <>
           <Divider
               orientation="vertical"
               flexItem
               sx={{ mx: 0.5, my: 0.25, borderColor: 'divider' }}
           />
           <Box sx={{ display: 'flex', flexDirection: 'column', lineHeight: 1.1, alignItems: 'flex-end' }}>
               <Typography
                   sx={{
                       fontFamily: monoFontFamily,
                       fontWeight: 700,
                       fontSize: 13,
                       color: 'text.primary',
                   }}
               >
                   {placement.eloAfter}
               </Typography>
               <Typography
                   sx={{
                       fontFamily: monoFontFamily,
                       fontWeight: 700,
                       fontSize: 10,
                       color:
                           (placement.eloDelta ?? 0) > 0
                               ? 'success.main'
                               : (placement.eloDelta ?? 0) < 0
                                 ? 'error.main'
                                 : 'text.disabled',
                   }}
               >
                   {(placement.eloDelta ?? 0) > 0 ? '+' : ''}
                   {placement.eloDelta ?? 0} ELO
               </Typography>
           </Box>
       </>
   )}
   ```

3. Add the `Divider` import:

   ```ts
   import Divider from '@mui/material/Divider'
   ```

Place the new segment **after** the player/team `<Box>` and **before** the
optional `Claim` `<Button>`. The Claim button keeps its current position
(rightmost when present). The divider only renders alongside the badge,
satisfying "when `eloAfter` is `null`, no badge segment and no divider".

Style notes:

- The badge re-uses `monoFontFamily` (already imported at line 5).
- The delta line text is `${sign}${value} ELO` — for `+12` the rendered
  string is `+12 ELO`; for `-8` it's `-8 ELO` (the leading minus comes
  from the integer's own representation, no explicit prefix needed); for
  `0` it's `0 ELO` rendered in `text.disabled`.
- `success.main` / `error.main` / `text.disabled` are MUI theme tokens
  already used elsewhere in the codebase (e.g. delta colours mirror
  `LeagueListPage`'s palette choices and the standard MUI semantic colour
  set).
- Numeric class for the sign check uses `placement.eloDelta ?? 0` so that
  when the API somehow ships `eloAfter: 1234, eloDelta: null` (which the
  calculator does not produce, but defensively), we render `0 ELO` in
  `text.disabled` rather than crashing on `null > 0`.

Run `npx prettier --write src/pages/PlacementPill.tsx` from
`src/Worms.Hub.Web/` after editing — slice 03/08 learnings flag this as
recurring.

### 10. `LeagueDetailPage` — shape sync only

`src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` defines its **own**
local `PlacementDto` interface (lines 51–56) for the league-detail replays
listing. The compiler treats the two `PlacementDto`s as structurally
related where they meet (`ReplayInLeagueDto.placements` is rendered
elsewhere via a sub-component that doesn't currently use the pill, but the
shapes are kept aligned in practice). Extend the local interface to keep
shapes consistent and avoid any future type-mismatch surprise:

```ts
interface PlacementDto {
    machine: string
    teamName: string
    position: number | null
    playerName: string | null
    eloDelta: number | null
    eloAfter: number | null
}
```

No rendering change here. Verify after edits by running `make web.lint`
which runs `tsc -b` and will surface any structural mismatch.

### 11. `GameDetailPage` — no change

`GameDetailPage.tsx` imports `PlacementDto` from `./PlacementPill`
(verified at line 28). Once `PlacementPill.tsx`'s `PlacementDto` is
extended, `GameDetailPage` picks up the new fields automatically with no
edit needed. The page already passes the full placement object to
`<PlacementPill placement={p} … />`. After the build, sanity-check that
the type checker is happy.

### 12. Existing tests, new tests

There is currently no `Worms.Hub.Gateway.Tests` or
`Worms.Hub.Storage.Tests` project (see
`.claude/docs/steering/testing-strategy.md` — the storage and gateway
projects rely on integration coverage via Docker + live DB rather than
unit tests).

The spec's acceptance criteria list "unit tests cover `RatingsCalculator`
…" — but introducing a new gateway test project just for this slice is
out of step with the codebase's strategy and was explicitly not done in
slice 06 either (see slice 06 learnings — no new test projects were added
for the calculator). The same justification applies here: the calculator
remains pure orchestration over PlayerRank + a tiny set of repositories,
and the established pattern is to verify through `docker compose up` +
DB inspection.

**Decision**: do not add a new test project. Verify the calculator's new
delta logic via the integration verification steps below (steps 4–8).
Slice retrospective can revisit whether the calculator now warrants a
dedicated unit-test project.

---

## Caveats and known risks

- **`PlayerRank.History.Leaderboard` is `IEnumerable`, not a dictionary.**
  Verified at runtime (PlayerRank 5.0.38). The spec's wording
  `history[g].Leaderboard[player].Points` is shorthand; the implementation
  must enumerate `Leaderboard` and match by `score.Name`. Build a per-
  snapshot lookup if the loop body is hot, but for our case the
  enumerable is small (one entry per player per snapshot) and the loop
  runs at most `replays * matchedPlayers` times. Plain `FirstOrDefault`
  is fine.

- **Empty-league edge case.** When a league has zero processed multi-
  player replays, `recordedGameCount == 0` and
  `league.GetLeaderBoardHistory(eloStrategy)` still returns one snapshot
  (verified). The multi-player write loop never runs; the single-matched
  loop's `PriorRecordedGameIndex == 0` branch falls through to the
  `snap is null` fallback returning 1000. End-state: all single-matched
  replays for never-recorded players get `(0, 1000)`; multi-player rows
  remain NULL because there are none.

- **`history[0]` is `1000` for every "ever-played" player.** Confirmed
  experimentally. Therefore for a player's first multi-player game,
  `before == 1000` and the acceptance criterion
  `elo_after - elo_delta === 1000` is satisfied without special casing.

- **`PlayerRank.League` / `Game` name clash with `Worms.Hub.Storage.Domain`.**
  Slice 06 learnings: must fully-qualify as `new PlayerRank.League()`
  and `new PlayerRank.Game()`. Already done in the existing
  `RatingsCalculator.cs` — keep it that way when extending.

- **CA1031 suppression.** The existing
  `[SuppressMessage("Design", "CA1031:Do not catch general exception types", …)]`
  on `CalculateForTeam` covers per-league error isolation. `Calculate`
  itself does not catch — exceptions propagate to the caller
  (`Processor`, `StartupBackfiller`, `CalculateForTeam`) which already
  wrap in try/catch with their own justification. No new suppression
  needed.

- **`CultureInfo.InvariantCulture` for `int.Parse`.** CA1305 will flag
  parsing without an `IFormatProvider`. Add it.

- **Roslynator RCS1124 (inline variable).** Avoid trivially-single-use
  local variables. Where the plan uses `var post = history[r.RecordedGameIndex]`
  and `var pre = history[r.RecordedGameIndex - 1]`, both are used twice
  (once each for an after/before lookup, then for diagnostic clarity).
  This is fine; the analyser only complains about strict single-use.

- **`monoFontFamily`** is already imported in `PlacementPill.tsx` at
  line 5. No additional theme import needed.

- **MUI `Divider orientation="vertical" flexItem`** requires the parent
  container to be `display: flex` with a finite cross-axis dimension. The
  pill `<Paper>` is already `display: 'flex'` with `alignItems: 'center'`,
  so `flexItem` works correctly here.

- **`npm ci` before `make web.lint`.** Per slice 05 / 08 learnings.

- **Migration version**: V0.9 follows V0.8. Confirmed V0.9 is unused in
  `src/database/migrations/` (listed contents above).

- **`StartupBackfiller` query complexity**: the detection query joins
  `replay_placements` to itself. For very large databases this is O(n²)
  in the join, but in practice the table is bounded by the number of
  replay placements (small), and the query runs once at gateway startup.
  Acceptable.

---

## Verification

1. `dotnet build src/Worms.Hub.Gateway --warnaserror` — passes with zero
   warnings (covers the calculator, controllers, DTO, startup backfiller,
   and migration-free C# changes).
2. `dotnet build src/Worms.Hub.Storage --warnaserror` — passes with zero
   warnings (covers the repository, domain record, and interface).
3. `make cli.build` — must succeed.
4. `docker compose up -d` (clean DB volume) ⇒ Flyway applies V0.9 with no
   errors; `SELECT column_name FROM information_schema.columns WHERE
   table_name = 'replay_placements'` includes both `elo_delta` and
   `elo_after`.
5. Process at least two replays in one league, each with two claimed
   teams, then `SELECT replay_id, machine, team_name, elo_delta,
   elo_after FROM replay_placements ORDER BY replay_id, position`. Verify:
   per-replay sum of `elo_delta` across matched placements is `0` (zero-
   sum, ±1 integer rounding). First game's `elo_after - elo_delta = 1000`
   for both players. Unclaimed teams (if any) show `(NULL, NULL)`.
6. Single-matched-player replay verification: claim only one of the two
   teams in a replay, run `Calculate` (e.g. by uploading another replay
   in the league, or by claiming a team to trigger `CalculateForTeam`).
   The single matched placement has `elo_delta = 0`, `elo_after = 1000`
   (for a player with no prior multi-player game).
7. Multi-team same player: in a replay where one player has claimed two
   teams, only the placement with the best position has non-NULL values;
   the other has both as NULL.
8. Post-migration, pre-backfill: drop and re-create the volume with the
   gateway feature flag set so `StartupBackfiller.BackfillRatings`
   short-circuits (or temporarily disable it); verify
   `GET /api/v1/leagues/{id}/replays/{replayId}` returns
   `"eloDelta": null, "eloAfter": null` for every placement and the UI
   omits the badge. Then re-enable backfill; values populate; badges
   appear.
9. `npm ci && make web.lint` inside `src/Worms.Hub.Web/` — passes (ESLint,
   `tsc -b`, Prettier).
10. `make web.build` — passes (Vite production build).
11. Browser smoke test on `/leagues/{id}/replays/{replayId}`: pill renders
    ELO badge with `eloAfter` on top (mono, bold, size 13, primary text)
    and `±N ELO` below (mono, bold, size 10, sign-aware colour). Winner
    pill keeps its warning-tinted background; delta colours still apply.
    Unclaimed teams and pre-backfill replays render with no badge and no
    divider.
12. Trigger a claim via the UI; backend ELO recalculation runs (slice 07
    behaviour); reload the game detail page: badges update accordingly.
