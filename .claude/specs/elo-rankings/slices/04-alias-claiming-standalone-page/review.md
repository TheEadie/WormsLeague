# Review — Alias Claiming — Standalone Page

## Verdict

The implementation satisfies all acceptance criteria in the spec. Both .NET builds pass clean (`dotnet build --warnaserror` exits 0 for `Worms.Hub.Storage` and `Worms.Hub.Gateway`). Web lint passes clean (`make web.lint` exits 0, covering ESLint, `tsc -b`, and Prettier). There are no blockers. Two minor suggestions are noted below for correctness and future-safety, but neither blocks merging.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Migration: `players` and `teams` tables exist; `teams` pre-populated from `replay_placements` | MET | `V0.7__AddPlayersAndTeams.sql` creates both tables with correct columns and runs the `INSERT … SELECT DISTINCT … ON CONFLICT DO NOTHING` backfill |
| Worker extension: new `(machine, team_name)` pairs inserted on replay processing | MET | `Processor.cs:85–91` — loop over `replayModel.Placements` calling `teamsRepository.Upsert` gated on `IsTeamsEnabledAsync()` |
| GET /teams — populated: returns list with `id`, `machine`, `teamName`, `claimedBy`, `isMyTeam` | MET | `TeamsController.cs:16–26`; `TeamDtos.cs:7–21` |
| GET /teams — empty: returns `[]` with HTTP 200 | MET | `TeamsController.cs:24–25` — `Ok(teams.Select(...).ToList())` returns an empty list when no rows exist |
| GET /teams — schema not applied: returns HTTP 404 | MET | `TeamsController.cs:18–21` — `NotFound()` when `!IsTeamsEnabledAsync()` |
| PUT /teams/{id} claim — success: 200 empty body; player record created; GET shows `claimedBy`/`isMyTeam: true` | MET | `TeamsController.cs:44–59` |
| PUT /teams/{id} claim — idempotent: 200 with no duplicate player record | MET | `TeamsController.cs:52–58` — `GetByAuth0Subject` guard prevents duplicate player creation; `SetPlayerClaim` is a plain UPDATE |
| PUT /teams/{id} claim — conflict: 409 | MET | `TeamsController.cs:46–50` — `Conflict()` when claimed by a different subject |
| PUT /teams/{id} unclaim — success: 200 empty body; GET shows `claimedBy: null` | MET | `TeamsController.cs:62–69` |
| PUT /teams/{id} unclaim — forbidden: 403 | MET | `TeamsController.cs:63–67` — `Forbid()` when claimed by a different subject |
| PUT /teams/{id} — not found: 404 | MET | `TeamsController.cs:36–39` |
| PUT /teams/{id} — schema not applied: 404 | MET | `TeamsController.cs:31–34` |
| Web — sort order: unclaimed → mine → others; alpha by machine then teamName within groups | MET | `TeamsPage.tsx:25–29` (`sortGroup`) and `TeamsPage.tsx:94–103` (sort comparator) |
| Web — combined list: Claim button for unclaimed, Unclaim button for mine, "Claimed by" for others | MET | `TeamsPage.tsx:142–193` |
| Web — in-flight state: button disabled while request in flight | MET | `TeamsPage.tsx:147,170` — `disabled={pending.has(team.id)}` |
| Web — failure: inline error message specific to status, button re-enabled | MET | `TeamsPage.tsx:72–84`; `finally` block at `TeamsPage.tsx:85–91` removes from `pending` |
| Web — empty state: descriptive message | MET | `TeamsPage.tsx:120–124` |
| Web — load failure: generic error message instead of list | MET | `TeamsPage.tsx:116–118` |
| Web — auth guard: unauthenticated redirect | MET | `App.tsx:43–49` — `/teams` wrapped in `<RequireAuth>` |

## Scope

All files in the working tree match the plan's "Files to Create / Modify" table exactly. The additional change to `.claude/specs/elo-rankings/plan.md` (marking the slice checkbox as done) is a process artefact and is ignored per review rules.

No planned files are absent. No unplanned feature files are present.

The one deviation noted in `learnings.md` — replacing `useCallback` with a `refetchKey` counter pattern — is well-explained and aligns with the pattern already used in `GameDetailPage.tsx` and `LeagueListPage.tsx`.

## Blockers

None.

## Suggestions

#### S1 — `GetAll` on `TeamsController` makes an unawaited synchronous call while the method is `async`

- **File:** `src/Worms.Hub.Gateway/API/Controllers/TeamsController.cs:24`
- **Issue:** `teamsRepository.GetAll()` is synchronous. The method is `async Task<…>` only because of the `await featureFlags.IsTeamsEnabledAsync()` call — that is correct. However, if the repository is ever made async (e.g. to use `QueryAsync`), the call site at line 24 will silently drop the async result unless it is updated. Not a current bug, but a pattern that could mislead.
- **Fix:** No code change required now — flag as a reminder when evolving the repository interface to async. Alternatively, leave a comment explaining that `GetAll` is intentionally synchronous.
- **Decision:** Accept

#### S2 — Error display for "Claim" rows does not clear when the row is successfully claimed by a different mechanism

- **File:** `src/Worms.Hub.Web/src/pages/TeamsPage.tsx:80–81`
- **Issue:** After a successful `claim` or `unclaim`, `setRefetchKey` triggers a re-fetch. The `errors` map is not cleared on re-fetch — it is only cleared per-row at the start of a new `handleClaim` call (`TeamsPage.tsx:58–61`). If row id `N` had an error and the user successfully claims a *different* team, and the re-fetch changes the sort order such that a new team lands at the same DOM slot, the stale error for id `N` will disappear correctly (keyed by id, not DOM position). This is actually fine. However, a full page re-fetch after a successful mutation returns a fresh list in which the previously-errored row may have a new state, yet the `errors` map still holds the old error for that id. The error will only clear when the user clicks the button again. This could show a stale error against a row that has since changed state (e.g., if the team was claimed by someone else in the interim).
- **Fix:** In the `setRefetchKey` branch (line 81), also call `setErrors(new Map())` to clear all errors on a successful mutation before the re-fetch.
- **Decision:** Accept

## Nitpicks

#### N1 — `[PublicAPI]` on `ClaimTeamDto` is not strictly needed

- **File:** `src/Worms.Hub.Gateway/API/DTOs/TeamDtos.cs:23`
- **Issue:** `ClaimTeamDto` is `internal sealed` and is only constructed by ASP.NET Core's model binder via `[FromBody]`. The `[PublicAPI]` annotation suppresses "unused" warnings — correct. But the annotation is also on `TeamDto` which is serialised to JSON by ASP.NET — also correct. No issue, just noting both annotations are justified for different reasons.
- **Fix:** No change required.
- **Decision:** Accept

## Tests

No new tests were added in this slice. This is consistent with the testing strategy: the gateway, storage, and worker layers do not currently have dedicated unit-test projects, and the testing strategy doc says behaviour at those layers is exercised indirectly via integration tests. The `RequireAuth` component (which guards `/teams`) already has automated tests covering all branches in `RequireAuth.test.tsx`.

The missing coverage is noted for awareness — the business logic in `TeamsController` (conflict detection, forbidden check, idempotent player creation) is not unit tested. The testing strategy acknowledges this gap for the gateway layer and defers it to a future integration-test effort. This is not a blocker.

## Recommended Actions

- **S1** — Decline — The repository interfaces are synchronous throughout the codebase; this is a speculative concern that does not reflect a current issue. No code change is warranted.
- **S2** — Accept — The stale-error scenario is a realistic UX bug: a user sees an inline error, another player claims the team in the background, the user claims a different team successfully, the re-fetch happens, but the stale 403 error still shows against the now-gone row. Clearing `errors` on success is a two-character fix and prevents user confusion.
- **N1** — Decline — Observation only; both `[PublicAPI]` annotations are justified.
