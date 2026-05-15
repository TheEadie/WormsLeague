# Plan: Placement Display

## Context

This slice surfaces the finish-order data (persisted in slice 02) across three output channels: the CLI `get replays` command, the Web UI replay pages, and the Slack game-complete announcement. Slices 01 and 02 have already delivered:

- `ReplayResource.Placements: IReadOnlyCollection<Placement>` — populated by the log parser
- `Replay.Placements: IReadOnlyList<ReplayPlacement>?` — stored in the database and available on the `Replay` domain object
- `IFeatureFlags.IsPlacementsEnabledAsync()` — already implemented in `GatewayFeatureFlags`, gated on schema version >= 0.5

What remains is wiring this data through the API DTO, through the Slack announcer interface, and into the three display surfaces. All changes degrade gracefully to current behaviour when placements are `null` (either because the feature flag is off or because the replay predates schema v0.5).

**Scope decision — list vs single endpoint asymmetry:** Both `GET /api/v1/leagues/{id}/replays` (list) and `GET /api/v1/leagues/{id}/replays/{replayId}` (single) already return `ReplayDetailDto`. Extending `ReplayDetailDto` with `Placements` therefore covers both endpoints simultaneously. No asymmetry exists and none needs to be called out.

**Scope decision — `ReplayDto`:** `ReplayDto` is the response type for the replay upload POST. It does not change.

---

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs` | Add `PlacementDto` record and `Placements` field to `ReplayDetailDto`; update `FromDomain` to accept a `bool placementsEnabled` flag |
| `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` | Check `IsPlacementsEnabledAsync()` and pass the result into `ReplayDetailDto.FromDomain` |
| `src/Worms.Hub.Gateway/Announcers/IAnnouncer.cs` | Add `IReadOnlyList<PlacementInfo>? placements` parameter to `AnnounceGameComplete` |
| `src/Worms.Hub.Gateway/Announcers/Slack/SlackAnnouncer.cs` | Implement the new overload: use placements list when non-null, fall back to winner string |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Inject `IFeatureFlags`; pass placements (when enabled) or `null` (when disabled) to `AnnounceGameComplete` |
| `src/Worms.Hub.Gateway/ServiceRegistration.cs` | Verify `IFeatureFlags` is registered in `AddWorkerServices()` (already registered; confirm no change needed, or add `TryAddScoped` if missing) |
| `src/Worms.Cli/Resources/Replays/ReplayTextPrinter.cs` | Update list view (TEAMS column + conditional WINNER column) and detail view (Awards section) to use `Placements` when non-empty |
| `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx` | Extend `ReplayDetailDto` interface with `placements`; update hero-card team chips to show position-prefixed labels in position order when placements are present |
| `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` | Extend `ReplayInLeagueDto` interface with `placements`; update Players column to show position-prefixed chips in position order when placements are present, falling back to winner-first ordering |

---

## Implementation Details

### 1. `PlacementDto` and `ReplayDetailDto` extension

In `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs`, add a new DTO record for a single placement entry:

```csharp
[PublicAPI]
internal sealed record PlacementDto(string Machine, string TeamName, int Position)
{
    internal static PlacementDto FromDomain(ReplayPlacement p) =>
        new(p.Machine, p.TeamName, p.Position);
}
```

Extend `ReplayDetailDto` with a nullable `Placements` field:

```csharp
[PublicAPI]
internal sealed record ReplayDetailDto(
    string Id,
    string Name,
    string Status,
    DateTime? Date,
    string? Winner,
    IReadOnlyList<string>? Teams,
    IReadOnlyList<TurnDto>? Turns,
    IReadOnlyList<PlacementDto>? Placements)
```

Update `FromDomain` to accept a `bool placementsEnabled` parameter:

```csharp
internal static ReplayDetailDto FromDomain(Replay replay, ReplayResource? parsed, bool placementsEnabled)
{
    // ... existing Turns computation unchanged ...

    IReadOnlyList<PlacementDto>? placements = null;
    if (placementsEnabled && replay.Placements is { Count: > 0 })
    {
        placements = replay.Placements.Select(PlacementDto.FromDomain).ToList();
    }

    return new ReplayDetailDto(
        replay.Id,
        replay.Name,
        replay.Status,
        replay.Date,
        replay.Winner,
        replay.Teams,
        turns,
        placements);
}
```

Note: the placements come from the domain `Replay` object (which is populated from the database by the repository), not from `ReplayResource` (which is the parsed-from-log model). The two are consistent after slice 02, but the DB-persisted values in `Replay.Placements` are the authoritative source for API responses. The `parsed` (`ReplayResource`) argument remains for Turns computation only.

### 2. Controller update — `LeaguesController`

Both `GetReplays` and `GetReplay` call `ReplayDetailDto.FromDomain`. Both must check the feature flag and pass the result.

Since `IsPlacementsEnabledAsync()` returns the same value for the whole request, call it once before the loop (in `GetReplays`) or before the single call (in `GetReplay`). The controller already injects `IFeatureFlags featureFlags`.

```csharp
[HttpGet("{id}/replays")]
public async Task<ActionResult<IReadOnlyList<ReplayDetailDto>>> GetReplays(string id)
{
    var league = leaguesRepository.GetById(id);
    if (league is null)
    {
        return NotFound();
    }

    var placementsEnabled = await featureFlags.IsPlacementsEnabledAsync();

    return Ok(replaysRepository.GetByLeagueId(id).Select(replay =>
    {
        ReplayResource? parsed = null;
        if (!string.IsNullOrEmpty(replay.FullLog))
        {
            parsed = replayTextReader.GetModel(replay.FullLog);
        }
        return ReplayDetailDto.FromDomain(replay, parsed, placementsEnabled);
    }).ToList());
}
```

Apply the same pattern to `GetReplay`.

Note: the existing `GetReplays` action does not itself gate on `IsLeaguesEnabledAsync` (it does a `GetById` null check which gives the same effect), so no new gating is needed — just thread the placements flag through.

### 3. `IAnnouncer` interface extension and `SlackAnnouncer` implementation

**Domain type for announcer placements:** Define a small value type inside the Gateway namespace (e.g. in `IAnnouncer.cs` itself, or a sibling file) to decouple the announcer from `ReplayPlacement` in `Worms.Hub.Storage`:

```csharp
internal record PlacementInfo(string TeamName, int Position);
```

Place this record in `src/Worms.Hub.Gateway/Announcers/IAnnouncer.cs` (just above or below the interface declaration), since it is used only by the interface and its implementation.

Update the interface:

```csharp
internal interface IAnnouncer
{
    Task AnnounceGameStarting(string hostName);

    Task AnnounceGameComplete(string winner, IReadOnlyList<PlacementInfo>? placements = null);
}
```

Using a default parameter value of `null` means existing call sites that only pass `winner` continue to compile without changes. However, the `Processor` will be updated to pass placements explicitly (see section 4), so the default is just a safety net.

**`SlackAnnouncer` implementation:**

When `placements` is non-null, build a multi-line results block. The position-sorted list in `<position>: <name>` format, one per line:

```csharp
public async Task AnnounceGameComplete(string winner, IReadOnlyList<PlacementInfo>? placements = null)
{
    logger.LogInformation("Announcing game complete to Slack");

    string bodyText;
    string headerText;
    if (placements is not null && placements.Count > 0)
    {
        headerText = "Results:";
        bodyText = string.Join("\n", placements
            .OrderBy(p => p.Position)
            .Select(p => $"{p.Position}: {p.TeamName}"));
    }
    else
    {
        headerText = "Winner:";
        bodyText = winner;
    }

    var slackMessage = new SlackMessage(
        "Game Complete",
        $$"""
          [
              {
                  "type": "header",
                  "text": {
                      "type": "plain_text",
                      "text": "Mayhem complete",
                      "emoji": true
                  }
              },
              {
                  "type": "section",
                  "text": {
                      "type": "mrkdwn",
                      "text": "*{{headerText}}*\n{{bodyText}}"
                  }
              }
          ]
          """);
    await PostToSlack(slackMessage);
}
```

**Important:** The interpolated string for the Slack JSON block uses C# raw string literals with `$$` double-dollar prefix so that `{{` and `}}` are the escape sequences for literal `{` and `}` in JSON. However, `bodyText` may contain newlines (one per team) — Slack `mrkdwn` renders `\n` as a line break in `section` blocks. No special escaping is needed for team names in the plain-text case; if team names contain characters that are meaningful in `mrkdwn` (e.g. `*`, `_`), that is an existing limitation also present in the current winner display.

### 4. `Processor` update

`Worker/Processor.cs` currently calls `announcer.AnnounceGameComplete(replayModel.Winner)`. It needs to:

1. Inject `IFeatureFlags` (already available in `AddWorkerServices()` — confirmed in `ServiceRegistration.cs`).
2. Check `IsPlacementsEnabledAsync()` once.
3. Build a `IReadOnlyList<PlacementInfo>?` from `replayModel.Placements` (which is `IReadOnlyCollection<Placement>` from `Worms.Armageddon.Files`).
4. Pass it to `AnnounceGameComplete`.

```csharp
internal sealed class Processor(
    IMessageQueue<ReplayToUpdateMessage> messageQueue,
    IReplaysRepository replayRepository,
    ReplayFiles replayFiles,
    IAnnouncer announcer,
    IReplayTextReader replayTextReader,
    IFeatureFlags featureFlags,
    ILogger<Processor> logger)
```

After parsing the replay log, before announcing:

```csharp
var placementsEnabled = await featureFlags.IsPlacementsEnabledAsync();
IReadOnlyList<PlacementInfo>? placements = null;
if (placementsEnabled && replayModel.Placements.Count > 0)
{
    placements = replayModel.Placements
        .Select(p => new PlacementInfo(p.Team.Name, p.Position))
        .ToList();
}

await announcer.AnnounceGameComplete(replayModel.Winner, placements);
```

Add the necessary `using` for `Worms.Hub.Gateway.Announcers` (for `PlacementInfo`) at the top of `Processor.cs`.

### 5. CLI `ReplayTextPrinter` changes

The `ReplayTextPrinter` works on `LocalReplay`, which holds a `ReplayResource`. `ReplayResource.Placements` is `IReadOnlyCollection<Placement>` — already populated by the parser from the log file. The placements feature flag does **not** apply to the CLI (it is a local display of local files — no gateway feature flag is consulted). Placements are shown when `Placements` is non-empty; the current behaviour is shown when it is empty (an empty collection is the unprocessed/no-log-present fallback set in `LocalReplayRetriever`).

**List view changes** in `Print(TextWriter writer, IReadOnlyCollection<LocalReplay> resources, int outputMaxWidth)`:

- If any resource has non-empty `Placements`, use position-prefixed team strings in TEAMS and omit the WINNER column.
- If no resource has placements (all have empty `Placements` collections), show the current behaviour: plain team names and a WINNER column.
- Per-row: if a replay has non-empty placements, format TEAMS as `string.Join(", ", placements.OrderBy(p => p.Position).Select(p => $"{p.Position}: {p.Team.Name}"))`. If a replay has empty placements, format TEAMS as plain team names. This mixed state within a list is possible during the transition period; the WINNER column is omitted for the whole table when at least one row has placements.

Actually, the simplest consistent approach (and what the spec implies by column-level decisions): the WINNER column is shown when `null` placements; it is hidden when placements are available. Since different replays in the list may have different states (some with placements, some without), apply the rule per-row for TEAMS formatting, and omit the WINNER column from the whole table if **any** replay has placements.

Revised approach for simplicity and consistency: check each replay individually.

```csharp
public void Print(TextWriter writer, IReadOnlyCollection<LocalReplay> resources, int outputMaxWidth)
{
    var tableBuilder = new TableBuilder(outputMaxWidth);
    var hasAnyPlacements = resources.Any(x => x.Details.Placements.Count > 0);

    tableBuilder.AddColumn(
        "NAME",
        [.. resources.Select(x => x.Details.Date.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture))]);
    tableBuilder.AddColumn("CONTEXT", [.. resources.Select(x => x.Context)]);
    tableBuilder.AddColumn("PROCESSED", [.. resources.Select(x => x.Details.Processed.ToString())]);

    if (!hasAnyPlacements)
    {
        tableBuilder.AddColumn("WINNER", [.. resources.Select(x => x.Details.Winner)]);
    }

    tableBuilder.AddColumn(
        "TEAMS",
        [.. resources.Select(x =>
            x.Details.Placements.Count > 0
                ? string.Join(", ", x.Details.Placements.OrderBy(p => p.Position).Select(p => $"{p.Position}: {p.Team.Name}"))
                : string.Join(", ", x.Details.Teams.Select(t => t.Name)))]);

    var table = tableBuilder.Build();
    TablePrinter.Print(writer, table);
}
```

**Detail view changes** in `Print(TextWriter writer, LocalReplay resource, int outputMaxWidth)`:

In the Awards section, replace:

```csharp
writer.WriteLine("Awards:");
writer.WriteLine($"Winner: {resource.Details.Winner}");
```

with:

```csharp
writer.WriteLine("Awards:");
if (resource.Details.Placements.Count > 0)
{
    foreach (var placement in resource.Details.Placements.OrderBy(p => p.Position))
    {
        writer.WriteLine($"{placement.Position}: {placement.Team.Name}");
    }
}
else
{
    writer.WriteLine($"Winner: {resource.Details.Winner}");
}
```

### 6. Web UI — `GameDetailPage.tsx`

**TypeScript interface extension:**

Add a `placements` field to the local `ReplayDetailDto` interface:

```typescript
interface PlacementDto {
    machine: string
    teamName: string
    position: number
}

interface ReplayDetailDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
    turns: TurnDto[] | null
    placements: PlacementDto[] | null
}
```

**Hero card — team chips:** The current block renders `replay.teams` as plain chips. When `replay.placements` is non-null and non-empty, instead derive the chip labels from placements sorted by position:

```typescript
{/* Team chips */}
{(replay.placements !== null && replay.placements.length > 0
    ? replay.placements
        .slice()
        .sort((a, b) => a.position - b.position)
        .map((p) => `${p.position}: ${p.teamName}`)
    : (replay.teams ?? [])
).map((label) => (
    <Chip
        key={label}
        label={label}
        size="small"
        variant="outlined"
        sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
    />
))}
```

Remove the conditional wrapper `{replay.teams !== null && (...)}` — instead guard on whether we have either placements or teams. When placements are present, the outer condition can stay but also check `replay.placements`:

Actually, keep the existing guard structure but update the chip source:

```typescript
{(replay.teams !== null || replay.placements !== null) && (
    <Stack direction="row" spacing={1} sx={{ mb: 2.5, flexWrap: 'wrap' }} useFlexGap>
        {(replay.placements !== null && replay.placements.length > 0
            ? replay.placements
                .slice()
                .sort((a, b) => a.position - b.position)
                .map((p) => `${p.position}: ${p.teamName}`)
            : (replay.teams ?? [])
        ).map((label) => (
            <Chip key={label} label={label} size="small" variant="outlined"
                sx={{ fontFamily: monoFontFamily, fontSize: 11 }} />
        ))}
        {league.version !== null && (
            <Chip label={`Scheme v${league.version}`} size="small" variant="outlined"
                sx={{ fontFamily: monoFontFamily, fontSize: 11 }} />
        )}
    </Stack>
)}
```

Also remove the existing winner chip (the `replay.winner !== null` block with the warning/default chip) from the title row when placements are available, since placement order supersedes the single winner chip. Specifically:

```typescript
{replay.winner !== null && replay.placements === null && (
    <Chip
        label={replay.winner}
        color={replay.winner === 'Draw' ? 'default' : 'warning'}
        size="small"
        sx={{ fontWeight: 700 }}
    />
)}
```

This hides the winner chip when placements are shown, keeping the title row clean.

### 7. Web UI — `LeagueDetailPage.tsx`

**TypeScript interface extension:**

Add `placements` to the local `ReplayInLeagueDto` interface (and add `PlacementDto`):

```typescript
interface PlacementDto {
    machine: string
    teamName: string
    position: number
}

interface ReplayInLeagueDto {
    id: string
    name: string
    status: string
    date: string | null
    winner: string | null
    teams: string[] | null
    turns: TurnDto[] | null
    placements: PlacementDto[] | null
}
```

**Players column:** Replace the current winner-first sort + crown icon rendering with a placement-aware version. When `replay.placements` is non-null and non-empty, render chips in position order with `<position>: <name>` labels and no crown icon. When placements are null, keep the current behaviour (winner-first sort, crown icon for winner).

Replace the Players `<TableCell>` content:

```typescript
<TableCell>
    <Stack direction="row" spacing={0.5} sx={{ flexWrap: 'wrap' }} useFlexGap>
        {replay.placements !== null && replay.placements.length > 0
            ? replay.placements
                .slice()
                .sort((a, b) => a.position - b.position)
                .map((p) => (
                    <Chip
                        key={`${p.machine}-${p.teamName}`}
                        label={`${p.position}: ${p.teamName}`}
                        size="small"
                        variant="outlined"
                        sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                    />
                ))
            : replay.teams
                ?.slice()
                .sort((a) => (a === replay.winner ? -1 : 1))
                .map((team) => (
                    <Box key={team} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        {team === replay.winner && (
                            <WorkspacePremiumIcon sx={{ fontSize: 14, color: 'warning.main' }} />
                        )}
                        <Chip
                            label={team}
                            size="small"
                            variant="outlined"
                            sx={{ fontFamily: monoFontFamily, fontSize: 11 }}
                        />
                    </Box>
                ))}
    </Stack>
</TableCell>
```

Note: the `key` for placement chips uses `${p.machine}-${p.teamName}` to be unique across teams from different machines with the same name. The `WorkspacePremiumIcon` import in `LeagueDetailPage.tsx` is still needed for the fallback path, so do not remove it.

### 8. `ServiceRegistration.cs` — `IFeatureFlags` in worker

Verify that `IFeatureFlags` is already registered inside `AddWorkerServices()` in `src/Worms.Hub.Gateway/ServiceRegistration.cs`. From slice 02's implementation this is expected to already be in place (the backfill service uses it). If not present, add:

```csharp
builder.TryAddScoped<IFeatureFlags, GatewayFeatureFlags>();
```

with the `using Microsoft.Extensions.DependencyInjection.Extensions;` as noted in slice 02's learnings.

---

## Verification

1. **Build clean:** `dotnet build --warnaserror src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj` — must produce no errors or warnings.
2. **Build clean (CLI):** `dotnet build --warnaserror src/Worms.Cli/Worms.Cli.csproj` — must produce no errors or warnings.
3. **Web lint:** `make web.lint` — must pass (TypeScript type-check, ESLint, Prettier).
4. **Web build:** `make web.build` — must produce a bundle without errors.
5. **Unit tests:** `make cli.test.unit` — all tests pass.
6. **API contract:** With schema >= v0.5, `GET /api/v1/leagues/{id}/replays` and `GET /api/v1/leagues/{id}/replays/{replayId}` both return a `placements` array (non-null) for replays with placement data. With schema < v0.5 (or a replay with no placements stored), both return `placements: null`.
7. **CLI list view:** `worms get replays` with a processed replay shows `1: TeamA, 2: TeamB` in TEAMS and no WINNER column; with an unprocessed replay (no placements), shows plain team names and a WINNER column.
8. **CLI detail view:** `worms get replay <name>` for a processed replay with placements shows `1: TeamA` etc in the Awards section and no `Winner: X` line.
9. **Slack:** Processor update means the next time a replay is processed and placements are enabled, `AnnounceGameComplete` receives a non-null placements list and posts a `*Results:*` block. Verify by code review of the conditional logic and confirming no compilation errors.
