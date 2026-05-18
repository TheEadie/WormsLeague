# Plan: ELO on Alias Changes

## Context

This slice wires ELO recalculation into the claim and unclaim flow so that historical replay data is immediately reflected when a player registers or removes an alias. It builds on slice 06 (ELO rankings), which added `RatingsCalculator.Calculate(leagueId)`, the `player_ratings` table, `IReplaysRepository.GetByLeagueId`, and the `IFeatureFlags.IsEloRatingsEnabledAsync()` gate. No database schema changes are required — all the data needed (`replay_placements`, `replays.league_id`, `replays.status`) is already in place.

The three concrete changes are:
1. Add `GetAffectedLeagueIds(string machine, string teamName)` to `IReplaysRepository` and implement it in `ReplaysRepositoryV05`.
2. Add `CalculateForTeam(string machine, string teamName)` to `RatingsCalculator`, with per-league error handling and an `ILogger` for error output.
3. Call `CalculateForTeam` from `TeamsController.Put` after a successful claim or unclaim, guarded by `IsEloRatingsEnabledAsync()`, and register `RatingsCalculator` in `AddGatewayServices()` so the DI container can resolve it in API-only mode.

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Storage/Database/IReplaysRepository.cs` | Add `IReadOnlyList<string> GetAffectedLeagueIds(string machine, string teamName)` |
| `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs` | Implement `GetAffectedLeagueIds` via a targeted SQL query |
| `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs` | Add `ILogger<RatingsCalculator>` to constructor; add `CalculateForTeam(string machine, string teamName)` method |
| `src/Worms.Hub.Gateway/API/Controllers/TeamsController.cs` | Add `RatingsCalculator` constructor parameter; call `CalculateForTeam` after successful claim/unclaim, guarded by feature flag |
| `src/Worms.Hub.Gateway/ServiceRegistration.cs` | Register `RatingsCalculator` as `TryAddScoped` in `AddGatewayServices()` |

---

## Implementation Details

### 1. `IReplaysRepository` — new method signature

Add the following method to the interface in `src/Worms.Hub.Storage/Database/IReplaysRepository.cs`:

```csharp
IReadOnlyList<string> GetAffectedLeagueIds(string machine, string teamName);
```

The return type is `IReadOnlyList<string>` (consistent with the existing `GetByLeagueId` return type convention). The method returns the distinct league IDs of all processed replays that contain a placement with the given `(machine, teamName)` pair.

### 2. `ReplaysRepositoryV05` — implement `GetAffectedLeagueIds`

Add the implementation inside the existing `ReplaysRepositoryV05` class. The query joins `replay_placements` to `replays` to find distinct non-null `league_id` values where the placement matches the requested `(machine, team_name)` pair and the replay is `Processed`:

```csharp
public IReadOnlyList<string> GetAffectedLeagueIds(string machine, string teamName)
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    return [.. connection.Query<string>(
        "SELECT DISTINCT r.league_id "
        + "FROM replay_placements rp "
        + "JOIN replays r ON r.id = rp.replay_id "
        + "WHERE rp.machine = @machine AND rp.team_name = @teamName "
        + "AND r.status = 'Processed' "
        + "AND r.league_id IS NOT NULL",
        new { machine, teamName })];
}
```

The `IS NOT NULL` filter in SQL avoids returning nulls that would need filtering in C#, and the `DISTINCT` is done in the database. Dapper maps a single-column result to `string` directly (no DB record type is needed).

### 3. `RatingsCalculator` — add logger and `CalculateForTeam`

`RatingsCalculator` currently has no logger. Add `ILogger<RatingsCalculator>` as a constructor parameter. The error-log call in `CalculateForTeam` mirrors the pattern already used in `Processor.UpdateReplay` and `StartupBackfiller.BackfillRatings`.

Add the `CalculateForTeam` method to `RatingsCalculator`:

```csharp
[SuppressMessage("Design", "CA1031:Do not catch general exception types",
    Justification = "ELO calculation failure for one league must not block remaining leagues or the claim/unclaim operation")]
public void CalculateForTeam(string machine, string teamName)
{
    var leagueIds = replaysRepository.GetAffectedLeagueIds(machine, teamName);
    foreach (var leagueId in leagueIds)
    {
        try
        {
            Calculate(leagueId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", leagueId);
        }
    }
}
```

The `[SuppressMessage]` attribute is required because `CA1031` (do not catch general exception types) is a warning that becomes an error under `--warnaserror`. The justification should be specific to this context. This pattern exactly matches the suppression already applied to `Processor.UpdateReplay` and `StartupBackfiller.BackfillRatings`. The `using System.Diagnostics.CodeAnalysis;` namespace must be added to the file's usings if not already present — it is not currently present in `RatingsCalculator.cs`.

The updated constructor signature becomes:

```csharp
internal sealed class RatingsCalculator(
    IReplaysRepository replaysRepository,
    ITeamsRepository teamsRepository,
    IRatingsRepository ratingsRepository,
    ILogger<RatingsCalculator> logger)
```

### 4. `TeamsController` — inject `RatingsCalculator` and call after claim/unclaim

Add `RatingsCalculator ratingsCalculator` as a constructor parameter. After each successful path in `Put` — both the claim branch (after `teamsRepository.SetPlayerClaim(body.Id, player.AuthSubject)`) and the unclaim branch (after `teamsRepository.SetPlayerClaim(body.Id, null)`) — add the feature-gated recalculation call.

Because `Put` is already `async`, the call to `IsEloRatingsEnabledAsync()` fits without further refactoring.

The `team` variable (already read via `teamsRepository.GetById(body.Id)`) carries `team.Machine` and `team.TeamName` which are the values passed to `CalculateForTeam`. Verify that the `Team` domain record has both `Machine` and `TeamName` properties before relying on them (they are present: confirmed from the claim flow in the current `TeamsController`).

Insert the following block at the end of the `Put` action body, before `return Ok()`, replacing the single `return Ok()` with a shared post-action block. The cleanest structure is a label-free fall-through: restructure so both branches set their claim and then fall through to shared post-processing:

```csharp
[HttpPut]
public async Task<ActionResult> Put(ClaimTeamDto body)
{
    if (!await featureFlags.IsTeamsEnabledAsync())
    {
        return NotFound();
    }

    var team = teamsRepository.GetById(body.Id);
    if (team is null)
    {
        return NotFound();
    }

    var callerSubject = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (body.Claimed)
    {
        if (team.IsClaimedByAnother(callerSubject))
        {
            return Conflict();
        }

        var player = playersRepository.GetByAuthSubject(callerSubject!);
        if (player is null)
        {
            var displayName = body.DisplayName ?? ResolveDisplayName();
            player = playersRepository.Create(new Player(callerSubject!, displayName));
        }

        teamsRepository.SetPlayerClaim(body.Id, player.AuthSubject);
    }
    else
    {
        if (team.IsClaimedByAnother(callerSubject))
        {
            return Forbid();
        }

        teamsRepository.SetPlayerClaim(body.Id, null);
    }

    if (await featureFlags.IsEloRatingsEnabledAsync())
    {
        ratingsCalculator.CalculateForTeam(team.Machine, team.TeamName);
    }

    return Ok();
}
```

The `if (await featureFlags.IsEloRatingsEnabledAsync())` guard is consistent with how `Processor.UpdateReplay` and `StartupBackfiller.BackfillRatings` gate their ELO calls.

### 5. `ServiceRegistration` — register `RatingsCalculator` in `AddGatewayServices`

`RatingsCalculator` is currently only registered inside `AddWorkerServices`. The API gateway runs `AddGatewayServices` only (when `WORMS_HUB_GATEWAY=true`), so `TeamsController` would fail to resolve `RatingsCalculator` at request time. Add `TryAddScoped<RatingsCalculator>()` inside `AddGatewayServices`:

```csharp
public static IServiceCollection AddGatewayServices(this IServiceCollection builder)
{
    builder.AddWormsArmageddonFilesServices()
        .AddHttpClient()
        .AddScoped<IAnnouncer, Announcer>()
        .AddScoped<ReplayFileValidator>()
        .AddScoped<CliFileValidator>()
        .AddScoped<IFeatureFlags, GatewayFeatureFlags>();
    builder.TryAddScoped<RatingsCalculator>();
    return builder;
}
```

Use `TryAddScoped` (not `AddScoped`) so that in monolith mode — where both `AddGatewayServices` and `AddWorkerServices` are called — the second call is silently skipped rather than causing a duplicate registration.

`RatingsCalculator` depends on `IReplaysRepository`, `ITeamsRepository`, and `IRatingsRepository`. These are already in the DI container when `AddGatewayServices` runs because `Program.cs` line 50 calls `builder.Services.AddHubStorageServices().AddGatewayServices().AddQueueServices()` inside the `if (runGateway)` block — `AddHubStorageServices()` is called before `AddGatewayServices()`. No change is needed to `Program.cs` or to `AddGatewayServices` beyond adding `TryAddScoped<RatingsCalculator>()`.

### 6. No new test projects

Per the testing strategy, the CLI, hub gateway, queues, and storage projects do not currently have unit-test projects — behaviour at those layers is exercised via integration tests or the libraries above. This slice adds pure orchestration wiring (method delegation + DI). No new test project is required. If a future slice adds a dedicated gateway test project, the `CalculateForTeam` logic is simple enough to be covered by unit tests at that point.

---

## Verification

1. Build with `dotnet build --warnaserror src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj` and `dotnet build --warnaserror src/Worms.Hub.Storage/Worms.Hub.Storage.csproj` — both must succeed with zero warnings.
2. Build the full CLI suite: `make cli.build` — must succeed.
3. Run unit tests: `make cli.test.unit` — must pass (no existing tests are broken; no new tests are introduced that could fail).
4. Manual end-to-end verification via `docker compose up`: claim a team alias via `PUT /api/v1/teams` when the ELO feature is enabled, then query `SELECT * FROM player_ratings` — the player's rating must appear. Unclaim the same alias; `player_ratings` must no longer contain ELO contributions from that alias.
5. Verify feature-flag guard: if the schema version is below `0.8` (ELO not enabled), claim/unclaim still returns 200 and no call to `GetAffectedLeagueIds` is made.
6. Verify error isolation: if `Calculate(leagueId)` throws for one league, the error is logged (visible in gateway logs), and the HTTP response is still 200.
