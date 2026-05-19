# Review — Slack Post — League Leaderboard with ELO Changes

## Verdict

The implementation satisfies every acceptance criterion in the spec. The build is clean (`dotnet build --warnaserror` exits with 0 warnings, 0 errors) and all 309 unit tests pass. The two new files and three modified files match the plan exactly; the learnings note explains the one deviation (CA1305 `CultureInfo.InvariantCulture` usage). There are no blockers. Two suggestions are worth considering and one nitpick is noted.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| ELO enabled + rated players → leaderboard block in Slack message | MET | `SlackAnnouncer.cs:39–51` appends the block when `leaderboard?.Count > 0`; `Processor.cs:106–113` builds it after `Calculate()` |
| Participant with ELO change → row shows `(+N)` or `(-N)` | MET | `LeaderboardFormatter.cs:37–46` formats the delta; `Processor.cs:157–163` computes it from pre/post ratings |
| Participant with no ELO change → no delta shown | MET | `Processor.cs:162–164` sets `eloDelta = null` when delta is 0; `LeaderboardFormatter.cs:37` guards `not 0` |
| Two players equal ELO → shared rank number | MET | `Processor.cs:150–153` only advances `rank` when rating is strictly less than the previous; equal ratings keep the same rank |
| Any player's rank changed → `⇧N` or `⇩N` on their row | MET | `LeaderboardFormatter.cs:49–54` emits arrows; `Processor.cs:165–169` computes position change for all players in post-game ratings |
| Player's rank unchanged → no arrow | MET | `Processor.cs:167–170` sets `positionChange = null` when 0; `LeaderboardFormatter.cs:49` only formats when non-null |
| ELO calculation fails or ratings unreachable → message posts with failure note | MET | `Processor.cs:115–119` catches and sets `leaderboardFailureNote`; `SlackAnnouncer.cs:53–65` appends a section block with the note |
| No rated players in league → message posts without leaderboard section and without failure note | MET | `Processor.cs:110–113` sets `leaderboard = null` when count is 0; `SlackAnnouncer.cs:39` checks `leaderboard?.Count > 0` |
| ELO feature flag off → Slack message identical to current format | MET | `Processor.cs:102` guards the entire ELO block behind `IsEloRatingsEnabledAsync()`; `leaderboard` and `leaderboardFailureNote` remain null |
| Replay has no league ID → Slack message identical to current format | MET | `Processor.cs:102` also guards on `updatedReplay.LeagueId is not null` |
| Mockup structure matches | MET | `LeaderboardFormatter.cs:28` produces `{rank}: {elo} {safeName}{suffix}` with `PadLeft` widths derived from the widest values; rank and ELO alignment match the spec example |

## Scope

All files in the working-tree diff belong to the plan's "Files to Create / Modify" table:

| File | Plan status |
|---|---|
| `src/Worms.Hub.Gateway/Announcers/LeaderboardEntry.cs` | New — matches plan |
| `src/Worms.Hub.Gateway/Announcers/LeaderboardFormatter.cs` | New — matches plan |
| `src/Worms.Hub.Gateway/Announcers/IAnnouncer.cs` | Modified — matches plan |
| `src/Worms.Hub.Gateway/Announcers/Slack/SlackAnnouncer.cs` | Modified — matches plan |
| `src/Worms.Hub.Gateway/Worker/Processor.cs` | Modified — matches plan |

No files outside the plan appear in the diff. The `CultureInfo.InvariantCulture` additions are explained in `learnings.md` and are required to satisfy CA1305 under `--warnaserror`; they are consistent with repo conventions.

## Blockers

None.

## Suggestions

#### S1 — Missing unit tests for `LeaderboardFormatter` and `BuildLeaderboard`

- **File:** `src/Worms.Hub.Gateway/Announcers/LeaderboardFormatter.cs`, `src/Worms.Hub.Gateway/Worker/Processor.cs:140–193`
- **Issue:** `LeaderboardFormatter.Format` and the private `BuildLeaderboard`/`ComputeRanks` methods in `Processor` contain non-trivial logic (tied-rank handling, sign convention, delta suppression, display-name escaping) with no automated tests. The testing-strategy doc notes the Gateway has no `*.Tests` project and explicitly says "when adding meaningful logic at those layers, prefer adding a new `<Project>.Tests` rather than retrofitting the integration test."
- **Fix:** Create `src/Worms.Hub.Gateway.Tests` (NUnit + Shouldly) and add `LeaderboardFormatterShould` covering at minimum: single player, tied ranks, delta formatting `(+N)`/`(-N)`, arrow formatting `⇧N`/`⇩N`, zero-delta suppression, zero-position-change suppression, and display-name escaping. `BuildLeaderboard` logic can be covered via `Processor` tests or extracted to a testable static helper.
- **Decision:** Decline — unit test project deferred to a follow-up PR.

#### S2 — `leaderboardFailureNote` is interpolated directly into raw JSON without escaping

- **File:** `src/Worms.Hub.Gateway/Announcers/Slack/SlackAnnouncer.cs:61`
- **Issue:** The failure note string `"ELO leaderboard unavailable."` is hardcoded today and contains no characters that could corrupt the JSON. However, the pattern at line 61 (`"text": "{{leaderboardFailureNote}}"`) performs no JSON escaping on the parameter, so any future change to the failure note that introduces `"`, `\`, or a newline would silently produce invalid JSON sent to Slack. The leaderboard `text` value at line 48 is protected by `LeaderboardFormatter`'s display-name escaping, but the failure note has no such guard.
- **Fix:** Either always use the current hardcoded constant (an `internal static` string in `Processor` would make the coupling explicit) or run the failure note string through `JsonSerializer.Serialize(leaderboardFailureNote)[1..^1]` before interpolation — the same caution the plan applied to display names.
- **Decision:** Accept

## Nitpicks

#### N1 — `⇩` arrow uses `{c}` not `{Math.Abs(c)}`

- **File:** `src/Worms.Hub.Gateway/Announcers/LeaderboardFormatter.cs:53`
- **Issue:** The "fell" case (positive `c`) uses `c.ToString(...)` rather than `Math.Abs(c).ToString(...)`. Since `c` is positive in that branch, both produce the same output, but `Math.Abs(c)` makes the intent symmetric with the improved-case at line 52 and removes the implicit reliance on the sign invariant.
- **Fix:** Change `$" ⇩{c.ToString(CultureInfo.InvariantCulture)}"` to `$" ⇩{Math.Abs(c).ToString(CultureInfo.InvariantCulture)}"`.
- **Decision:** Accept

## Tests

No new tests were added. The existing 309 unit tests all pass and none touch the new code. The testing-strategy doc explicitly acknowledges the Gateway has no unit-test project and suggests creating one when meaningful logic is added. Given the non-trivial edge cases in `LeaderboardFormatter` and `BuildLeaderboard` (tied ranks, zero-delta suppression, display-name escaping), the absence of tests is worth addressing before this ships — see S1.

## Recommended Actions

- **S1** — Accept — `LeaderboardFormatter` and `BuildLeaderboard` contain enough branching logic (tied ranks, sign conventions, zero suppression, escaping) that the first regression will be hard to diagnose without a test harness. The testing-strategy doc explicitly calls for a new `<Project>.Tests` in this situation.
- **S2** — Accept — The risk is low today (hardcoded string), but the pattern is fragile. Establishing either a named constant or explicit escaping costs almost nothing and prevents a silent breakage if the message is ever changed.
- **N1** — Accept — One-character fix; makes the two branches symmetric and removes a subtle correctness dependency on the caller upholding the sign contract.
