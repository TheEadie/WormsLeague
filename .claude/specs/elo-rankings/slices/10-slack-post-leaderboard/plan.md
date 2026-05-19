# Plan: Slack Post — League Leaderboard with ELO Changes

## Context

This slice extends the "Mayhem complete" Slack message to include a formatted
leaderboard block showing all rated players in the league after each game is
processed. It builds directly on slice 06 (ELO rankings — `RatingsCalculator`
and `IRatingsRepository` in place), slice 07 (recalculation on alias changes),
and slice 09 (ELO delta per-placement stored on the replay). No database
migrations, no new API endpoints, and no Web UI changes are required.

The `Processor` in `Worms.Hub.Gateway/Worker/Processor.cs` already calls
`ratingsCalculator.Calculate(leagueId)` before calling
`announcer.AnnounceGameComplete(...)`. This ordering is preserved: ratings are
updated first, then the Slack post is built and sent. To compute rank position
changes the Processor must read the pre-game ratings from `IRatingsRepository`
before calling `Calculate()`, then read the post-game ratings immediately after.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/Worms.Hub.Gateway/Announcers/LeaderboardEntry.cs` | Immutable record carrying the data for one player's leaderboard row: rank, ELO, display name, ELO delta (nullable int), and rank position change (nullable int, negative = improved). |
| `src/Worms.Hub.Gateway/Announcers/LeaderboardFormatter.cs` | Pure static class that turns `IReadOnlyList<LeaderboardEntry>` into the fixed-width code-block text that goes inside the Slack block. |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Gateway/Announcers/IAnnouncer.cs` | Add `IReadOnlyList<LeaderboardEntry>? leaderboard = null` optional parameter to `AnnounceGameComplete`. |
| `src/Worms.Hub.Gateway/Announcers/Slack/SlackAnnouncer.cs` | When `leaderboard` is non-null and non-empty, append a second Slack block (section with mrkdwn using a code fence) containing the formatted leaderboard text. |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Inject `IRatingsRepository`. Before calling `Calculate()`, snapshot pre-game ratings. After `Calculate()`, read post-game ratings and build `IReadOnlyList<LeaderboardEntry>`. Pass the list to `AnnounceGameComplete`. The leaderboard build is wrapped in its own try-catch so failures produce a failure-note leaderboard rather than preventing the Slack post. |

---

## Implementation Details

### 1. New type: `LeaderboardEntry`

Create `src/Worms.Hub.Gateway/Announcers/LeaderboardEntry.cs`:

```csharp
namespace Worms.Hub.Gateway.Announcers;

/// <summary>
/// One row in the post-game leaderboard Slack block.
/// </summary>
/// <param name="Rank">1-based rank number; shared by players with equal ELO.</param>
/// <param name="Elo">Current ELO rating.</param>
/// <param name="DisplayName">Player display name.</param>
/// <param name="EloDelta">
/// Change in ELO from this game. Null if the player did not participate
/// or their rating did not change.
/// </param>
/// <param name="PositionChange">
/// Change in rank position (positive = fell, negative = improved).
/// Null if the rank did not change. Zero is never stored — use null for no change.
/// </param>
internal sealed record LeaderboardEntry(
    int Rank,
    int Elo,
    string DisplayName,
    int? EloDelta,
    int? PositionChange);
```

The `PositionChange` sign convention (positive = fell) mirrors how a rank number
increases when you fall: if you were rank 2 and are now rank 4 the change is +2
(fell), shown as `⇩2`. If you were rank 4 and are now rank 2 the change is -2
(improved), shown as `⇧2`.

### 2. New class: `LeaderboardFormatter`

Create `src/Worms.Hub.Gateway/Announcers/LeaderboardFormatter.cs`:

```csharp
using System.Text;

namespace Worms.Hub.Gateway.Announcers;

internal static class LeaderboardFormatter
{
    public static string Format(IReadOnlyList<LeaderboardEntry> entries)
    {
        // Determine column widths dynamically
        var rankWidth  = entries.Max(e => e.Rank.ToString().Length);
        var eloWidth   = entries.Max(e => e.Elo.ToString().Length);

        var sb = new StringBuilder();
        sb.AppendLine("Leaderboard:");

        foreach (var entry in entries)
        {
            var rank = entry.Rank.ToString().PadLeft(rankWidth);
            var elo  = entry.Elo.ToString().PadLeft(eloWidth);

            var suffix = BuildSuffix(entry);

            sb.AppendLine($"{rank}: {elo} {entry.DisplayName}{suffix}");
        }

        // Trim trailing newline — Slack adds its own spacing.
        return sb.ToString().TrimEnd();
    }

    private static string BuildSuffix(LeaderboardEntry entry)
    {
        var delta = entry.EloDelta is { } d and not 0
            ? d > 0 ? $" (+{d})" : $" ({d})"
            : string.Empty;

        var arrow = entry.PositionChange is { } c
            ? c < 0 ? $" ⇇1{Math.Abs(c)}" : $" ⇉1{Math.Abs(c)}"
            : string.Empty;
        // ⇧ = U+21C7 (improvement, rank number fell), ⇩ = U+21C9 (fell, rank number rose)
        // Correct Unicode: ⇧ = U+21E7, ⇩ = U+21E9 — see note below.

        return delta + arrow;
    }
}
```

**Arrow Unicode correction**: The spec uses `⇧` (U+21E7, UPWARDS WHITE ARROW)
for an improving rank and `⇩` (U+21E9, DOWNWARDS WHITE ARROW) for a declining
rank. Use these code points literally in the source string (not `⇇`):

```csharp
c < 0 ? $" ⇧{Math.Abs(c)}" : $" ⇩{Math.Abs(c)}"
```

The `Format` method returns the plain text that will be wrapped in triple
backticks by `SlackAnnouncer`. Do not include the backticks here.

Tie-rank: entries are passed in already carrying the correct `Rank` value
(shared for equal ELO), so `Format` does not need to re-compute ranks.

ELO delta of zero: the spec says "no delta shown if ELO did not change". A
delta of exactly zero is treated as no change (same as null). The `EloDelta`
stored in `LeaderboardEntry` will be null in that case (see §4 below), but
as a belt-and-suspenders check the formatter guards `not 0` too.

### 3. Extend `IAnnouncer`

```csharp
internal interface IAnnouncer
{
    Task AnnounceGameStarting(string hostName);

    Task AnnounceGameComplete(
        string winner,
        IReadOnlyList<PlacementInfo>? placements = null,
        IReadOnlyList<LeaderboardEntry>? leaderboard = null);
}
```

The new parameter is optional so all existing callers compile without change.

### 4. Extend `SlackAnnouncer.AnnounceGameComplete`

When `leaderboard` is non-null and non-empty, append a second Slack block to the
JSON blocks array. The second block is a `section` with `mrkdwn` text containing
the formatted leaderboard wrapped in triple backticks:

```csharp
public async Task AnnounceGameComplete(
    string winner,
    IReadOnlyList<PlacementInfo>? placements = null,
    IReadOnlyList<LeaderboardEntry>? leaderboard = null)
{
    // ... existing headerText / bodyText logic unchanged ...

    var leaderboardBlock = string.Empty;
    if (leaderboard?.Count > 0)
    {
        var text = LeaderboardFormatter.Format(leaderboard);
        leaderboardBlock = $$"""
            ,
                {
                    "type": "section",
                    "text": {
                        "type": "mrkdwn",
                        "text": "```{{text}}```"
                    }
                }
            """;
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
              }{{leaderboardBlock}}
          ]
          """);
    await PostToSlack(slackMessage);
}
```

The `SlackAnnouncer` file already uses the `Announcer` class name (not
`SlackAnnouncer`) — keep that as-is.

**Failure note block**: When the leaderboard cannot be produced (see §5), the
`Processor` passes a single sentinel entry. Instead, use a dedicated
`failureNote` string that is handled separately. See §5 for the actual
approach: pass `null` leaderboard on failure and add a distinct failure-note
path.

**Revised approach for failure note**: Add an overload-friendly nullable
`string? leaderboardFailureNote = null` parameter, OR handle it entirely in
`Processor` by constructing a special leaderboard list. The cleanest option
that avoids proliferating parameters:

Add a second optional parameter `string? leaderboardFailureNote = null` to
`AnnounceGameComplete`. When this is non-null, append a section block with
that text in place of the formatted leaderboard. When both `leaderboard` and
`leaderboardFailureNote` are null, no extra block is appended.

```csharp
// Revised IAnnouncer:
Task AnnounceGameComplete(
    string winner,
    IReadOnlyList<PlacementInfo>? placements = null,
    IReadOnlyList<LeaderboardEntry>? leaderboard = null,
    string? leaderboardFailureNote = null);
```

In `SlackAnnouncer`:
- If `leaderboard?.Count > 0`: format and append the code-block section.
- Else if `leaderboardFailureNote is not null`: append a plain section with the failure note text.
- Else: no extra block.

The failure note text from `Processor` is `"ELO leaderboard unavailable."`.

### 5. Extend `Processor` to build the leaderboard

The `Processor` needs `IRatingsRepository` injected alongside `RatingsCalculator`.
It is already registered in the DI container via `AddHubStorageServices()`.

Changes to `Processor`:

1. Add `IRatingsRepository ratingsRepository` to the primary constructor.
2. Inside the `if (await featureFlags.IsEloRatingsEnabledAsync() && updatedReplay.LeagueId is not null)` block:
   - Before calling `ratingsCalculator.Calculate(...)`, read the pre-game ratings:
     `var preGameRatings = ratingsRepository.GetByLeagueId(updatedReplay.LeagueId);`
   - Call `ratingsCalculator.Calculate(updatedReplay.LeagueId);` (unchanged).
   - After `Calculate`, read the post-game ratings:
     `var postGameRatings = ratingsRepository.GetByLeagueId(updatedReplay.LeagueId);`
   - Build the leaderboard entries using a helper method (see below).
3. Pass the leaderboard (or failure note) to `announcer.AnnounceGameComplete(...)`.

The existing `catch` block that logs and swallows ELO calculation failures must
be extended: on any exception, set `leaderboardFailureNote` instead of leaving
`leaderboard` populated, and always continue to `AnnounceGameComplete`.

**Complete revised ELO block in `Processor.UpdateReplay`**:

```csharp
IReadOnlyList<LeaderboardEntry>? leaderboard = null;
string? leaderboardFailureNote = null;

if (await featureFlags.IsEloRatingsEnabledAsync() && updatedReplay.LeagueId is not null)
{
    try
    {
        var preGameRatings = ratingsRepository.GetByLeagueId(updatedReplay.LeagueId);
        ratingsCalculator.Calculate(updatedReplay.LeagueId);
        var postGameRatings = ratingsRepository.GetByLeagueId(updatedReplay.LeagueId);
        leaderboard = BuildLeaderboard(preGameRatings, postGameRatings);
        if (leaderboard.Count == 0)
        {
            leaderboard = null; // empty leaderboard — omit the block entirely
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to calculate ELO ratings for league {LeagueId}.", updatedReplay.LeagueId);
        leaderboardFailureNote = "ELO leaderboard unavailable.";
    }
}

// Announce game complete
// ... existing placements logic ...
await announcer.AnnounceGameComplete(replayModel.Winner, placements, leaderboard, leaderboardFailureNote);
```

**`BuildLeaderboard` private static method on `Processor`**:

```csharp
private static IReadOnlyList<LeaderboardEntry> BuildLeaderboard(
    IReadOnlyList<PlayerRating> preGameRatings,
    IReadOnlyList<PlayerRating> postGameRatings)
{
    // Post-game ratings come back ordered by rating DESC (from RatingsRepository).
    // Compute rank numbers (shared for equal ELO).
    var entries = new List<LeaderboardEntry>(postGameRatings.Count);
    var preBySubject = preGameRatings.ToDictionary(r => r.PlayerAuthSubject, r => r.Rating);
    var preRankBySubject = ComputeRanks(preGameRatings);

    var rank = 1;
    for (var i = 0; i < postGameRatings.Count; i++)
    {
        // Players with equal ELO share the same rank.
        if (i > 0 && postGameRatings[i].Rating < postGameRatings[i - 1].Rating)
        {
            rank = i + 1;
        }

        var post = postGameRatings[i];
        var eloDelta = preBySubject.TryGetValue(post.PlayerAuthSubject, out var preElo)
            ? post.Rating - preElo
            : (int?)null;
        if (eloDelta == 0)
        {
            eloDelta = null;
        }

        var preRank = preRankBySubject.GetValueOrDefault(post.PlayerAuthSubject, rank);
        var positionChange = rank - preRank;
        if (positionChange == 0)
        {
            positionChange = null;
        }

        entries.Add(new LeaderboardEntry(rank, post.Rating, post.DisplayName, eloDelta, positionChange));
    }

    return entries;
}

private static Dictionary<string, int> ComputeRanks(IReadOnlyList<PlayerRating> ratings)
{
    var result = new Dictionary<string, int>(ratings.Count);
    var rank = 1;
    for (var i = 0; i < ratings.Count; i++)
    {
        if (i > 0 && ratings[i].Rating < ratings[i - 1].Rating)
        {
            rank = i + 1;
        }
        result[ratings[i].PlayerAuthSubject] = rank;
    }
    return result;
}
```

**Rank position change sign convention** (confirmed for display):
- `positionChange = rank - preRank` where `rank` is the post-game rank.
- If a player's rank *number* increased (e.g., from 2 to 4), `positionChange` is positive (+2) → show `⇩2` (fell).
- If a player's rank *number* decreased (e.g., from 4 to 2), `positionChange` is negative (-2) → show `⇧2` (improved).

In `LeaderboardFormatter`:
```csharp
c < 0 ? $" ⇧{Math.Abs(c)}" : $" ⇩{c}"
```

**Players new to the leaderboard** (no pre-game rating): `preBySubject` will
not contain their `PlayerAuthSubject`, so `eloDelta` is null and `preRank`
defaults to `rank` (their current post-game rank), giving `positionChange = 0`
→ null. This matches the spec's "first game" out-of-scope clause.

**`[SuppressMessage]` placement**: The existing `[SuppressMessage("Design",
"CA1031:Do not catch general exception types", ...)]` attribute on `UpdateReplay`
already covers the ELO try-catch block. The new catch block structure must
remain inside that method, so no new suppress attribute is needed.

### 6. `LeaderboardFormatter` — fixed-width alignment detail

From the spec example:
```
1:  1033 Player A (+7)
2:  1021 Player B (+33) ⇧3
```

The rank number and ELO are padded to the width of the widest value in the
list. The format per row is:

```
{rank,rankWidth}: {elo,eloWidth} {DisplayName}{suffix}
```

Where `suffix` is the concatenation of `delta` (if non-null) and `arrow` (if
non-null), both with a leading space. Example: ` (+33) ⇧3`.

`StringBuilder.AppendLine` adds `\r\n` on Windows. Use `sb.Append(...)` +
`sb.Append('\n')` or call `TrimEnd()` on the final string and let the caller
handle trailing whitespace. The Slack renderer ignores trailing newlines inside
a code block. `TrimEnd()` on the final result is fine.

### 7. Slack code-block escaping

Slack's mrkdwn code fence uses triple backticks. The formatted text must not
contain triple backticks itself (it won't — ELO values and player names are
alphanumeric/punctuation only). No escaping is needed.

The text will be embedded in a JSON string literal inside the raw blocks JSON.
Any double-quote, backslash, or control character in `DisplayName` could
corrupt the JSON. Guard against this by escaping the display name when building
the formatted string:

```csharp
var safeName = entry.DisplayName
    .Replace("\\", "\\\\", StringComparison.Ordinal)
    .Replace("\"", "\\\"", StringComparison.Ordinal)
    .Replace("\n", " ", StringComparison.Ordinal);
```

Apply this in `LeaderboardFormatter.Format` when writing each row.

### 8. Caveats and known patterns from earlier slices

- **`internal sealed record`**: All new types must be `internal sealed` (Roslynator CA1852).
- **`CA1031` suppression**: Already on `UpdateReplay`. No new attribute needed for the ELO block since the try-catch remains inside that method.
- **`TryAddScoped` namespace**: `Microsoft.Extensions.DependencyInjection.Extensions` must be imported explicitly if `TryAddScoped` is added anywhere (not needed here since `IRatingsRepository` is already DI-registered).
- **`PlayerRank.League` vs `Worms.Hub.Storage.Domain.League` ambiguity**: Not a concern in this slice — no `PlayerRank` types used in the new code.
- **`Roslynator RCS1124`**: Inline single-use locals. In `BuildLeaderboard`, keep `preBySubject` and `preRankBySubject` as locals (each is used more than once). The `entries` list is only assigned once but is a collection built in a loop — not subject to RCS1124.

---

## Verification

1. Build with `dotnet build --warnaserror src/Worms.Hub.Gateway` — must produce zero warnings.
2. Run `make cli.test.unit` — all existing unit tests pass.
3. Stand up the local stack with `docker compose up` and upload a replay that belongs to a league where at least two players have claimed aliases and ratings exist. Inspect the Slack webhook payload (via a tool like `ngrok` or a local webhook.site redirect). Confirm:
   - The JSON blocks array has two entries: the existing "Mayhem complete" header + results section, plus a new section with mrkdwn text containing triple-backtick-fenced leaderboard text.
   - Each rated player appears in rank order with correct ELO.
   - Players whose ELO changed show `(+N)` or `(-N)`.
   - Players whose rank position changed show `⇧N` or `⇩N`.
4. Process a replay for a league with no rated players — confirm the Slack message contains only the two original blocks (header + results), no third block, no failure note.
5. Confirm the ELO feature flag path: set `WORMS_HUB_ELO_DISABLED=true` (or use a schema version below 0.8 in local dev) and process a replay — confirm the Slack message is identical to the pre-ELO format (no leaderboard block, no failure note).
6. Confirm the failure-note path: in a local dev build, temporarily make `ratingsRepository.GetByLeagueId` throw, process a replay for a league with rated players, and confirm the Slack message posts successfully with the failure note text `"ELO leaderboard unavailable."` in place of the leaderboard block.
