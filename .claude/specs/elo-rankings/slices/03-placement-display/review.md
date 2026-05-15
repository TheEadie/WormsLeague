# Review — Placement Display

## Verdict

The implementation satisfies all acceptance criteria. All three builds (Hub Gateway, CLI, Web UI) exit clean with zero warnings, all 309 unit tests pass, and `make web.lint` (ESLint, `tsc -b`, Prettier) passes. There are no blockers. One suggestion and one nitpick are raised, both minor.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| `GET /api/v1/leagues/{id}/replays` and `GET /api/v1/leagues/{id}/replays/{replayId}` include a non-null `placements` array when schema >= v0.5 and placements stored | MET | `ReplayDtos.cs:58-62` — placements populated when `placementsEnabled && replay.Placements is { Count: > 0 }`; `LeaguesController.cs:80,108` — `IsPlacementsEnabledAsync()` called and threaded into `FromDomain` |
| Both endpoints return `placements: null` when schema < v0.5 | MET | `ReplayDtos.cs:58` — `placementsEnabled` is `false` when flag off, so `placements` stays `null` |
| CLI list: `[{TeamA, 1}, {TeamB, 2}, {TeamC, 3}]` → TEAMS shows `1: TeamA, 2: TeamB, 3: TeamC`, no WINNER column | MET | `ReplayTextPrinter.cs:21-24` — WINNER column omitted when `hasAnyPlacements`; `ReplayTextPrinter.cs:29-30` — ordered placement format applied per row |
| CLI list: tied placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 2}]` → TEAMS shows `1: TeamA, 2: TeamB, 2: TeamC` | MET | `ReplayTextPrinter.cs:30` — `OrderBy(p => p.Position).Select(p => $"{p.Position}: {p.Team.Name}")` preserves repeated position numbers |
| CLI list: `null` placements → plain team names, WINNER column present | MET | `ReplayTextPrinter.cs:21-24, 31` — WINNER column added when `!hasAnyPlacements`; plain team names used when `Placements.Count == 0` |
| CLI detail: placements → placement list per line, no "Winner:" line | MET | `ReplayTextPrinter.cs:74-79` — `foreach` over ordered placements; else branch writes `Winner:` line |
| CLI detail: `null` placements → "Winner: X" line, no placement list | MET | `ReplayTextPrinter.cs:81-83` — else branch used when `Placements.Count == 0` |
| GameDetailPage: placements present → chips in position order with `<position>: ` prefix | MET | `GameDetailPage.tsx:520-524` — `.sort((a, b) => a.position - b.position).map((p) => \`${p.position}: ${p.teamName}\`)` |
| GameDetailPage: tied placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 2}]` → chips `1: TeamA`, `2: TeamB`, `2: TeamC` | MET | Same code as above; tied positions produce repeated prefix |
| GameDetailPage: `null` placements → plain team name chips, no prefix, no reordering | MET | `GameDetailPage.tsx:525` — fallback `(replay.teams ?? [])` used when placements null |
| LeagueDetailPage: placements present → chips in position order with `<position>: ` prefix | MET | `LeagueDetailPage.tsx:254-275` — placement path renders `\`${p.position}: ${p.teamName}\`` sorted by position |
| LeagueDetailPage: tied placements → chips show `1: TeamA`, `2: TeamB`, `2: TeamC` | MET | Same code path; repeated position numbers preserved |
| LeagueDetailPage: `null` placements → winner-first order with plain chip labels and crown icon | MET | `LeagueDetailPage.tsx:276-313` — fallback path retains existing winner-first sort and `WorkspacePremiumIcon` |
| Slack: placements present → `Results:` header + one `<position>: <name>` entry per line | MET | `SlackAnnouncer.cs:21-27` — `headerText = "Results:"`, `bodyText` built with `OrderBy(p => p.Position).Select(p => $"{p.Position}: {p.TeamName}")` |
| Slack: tied placements → `2: TeamB` and `2: TeamC` on separate lines | MET | Same join path; tied entries preserve the repeated position number |
| Slack: `null` placements → winner only (current behaviour) | MET | `SlackAnnouncer.cs:28-32` — `headerText = "Winner:"`, `bodyText = winner` |

## Scope

All nine modified files match the plan's "Files to Create / Modify" table exactly. No unplanned files appear in the diff; no planned files are absent. The `learnings.md` explains all deviations from the plan:

- `PlacementInfo` declared `internal sealed record` (CA1852 compliance)
- `if (placements is not null && placements.Count > 0)` changed to `if (placements?.Count > 0)` (RCS1146 compliance)
- `GetReplays` and `GetReplay` made `async` (required to `await` the feature flag check)
- Prettier reformatted TSX indentation

No unexplained deviations.

## Blockers

None.

## Suggestions

#### S1 — `GameDetailPage` chip key uses label string rather than stable machine+team key

- **File:** `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx:528`
- **Issue:** The `key` for placement chips is the label string `\`${p.position}: ${p.teamName}\``. This is unique within a game (position + team name together cannot collide), so no React warning occurs. However, `LeagueDetailPage.tsx:265` uses `\`${p.machine}-${p.teamName}\`` — the plan's recommended approach — for consistency and to guard against future edge cases where two teams share a name from different machines.
- **Fix:** Change the key to `` `${p.machine}-${p.teamName}` `` (matching `LeagueDetailPage`) or `` `${p.position}-${p.teamName}` `` to make the key stable and independent of the label format.
- **Decision:** Accept

## Nitpicks

#### N1 — `hub-gateway` component doc still describes `AnnounceGameComplete(winner)` as a single-argument method

- **File:** `.claude/docs/components/hub-gateway.md:44`
- **Issue:** The doc says `AnnounceGameComplete(winner)`, which is now `AnnounceGameComplete(winner, placements?)`. This is not a code defect, but a stale doc sentence that will mislead the next developer working in this area.
- **Fix:** Update the `Announcers` paragraph in the component doc to reflect the new signature.
- **Decision:** Accept

## Tests

No new test code was added. This is consistent with the existing pattern in this repo: neither `Worms.Cli` nor `Worms.Hub.Gateway` has a unit-test project. The testing strategy doc explicitly states that behaviour at those layers is exercised indirectly or via integration tests, and that new `<Project>.Tests` projects should be added when meaningful logic is introduced.

The conditional logic in `ReplayTextPrinter` is the most testable new behaviour (pure function on collection → formatted string). The other changes (controller, DTO factory, Slack announcer, web components) all have the same indirect coverage as before. No coverage gap is introduced beyond what already existed.

No padding tests or fragile patterns to flag — there are no new tests.

## Recommended Actions

- **S1** — Decline — The current `label`-as-key is safe for this component (position + team name is unique within a single game replay), and fixing it would be pure consistency cleanup with no user-visible impact. Worth doing opportunistically but not as a blocker.
- **N1** — Accept — The component doc sentence is clearly stale after this change; a one-line fix prevents future confusion when anyone reads the doc before touching the announcer.
