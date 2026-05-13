# Review — Game Detail Page

## Verdict

The implementation satisfies the great majority of the spec. The build and lint pass clean, the SPA component is well-structured, the DTO mapping is correct, and the seed data will parse with the existing `IReplayTextReader`. There is one blocker: removing `AddWormsArmageddonFilesServices()` from `AddWorkerServices()` and placing it only inside the `if (runGateway)` block in `Program.cs` breaks the distributed worker-only deployment mode — `Processor` depends on `IReplayTextReader`, which is no longer registered when the gateway does not run. This must be fixed before merging.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Signed-in user sees hero card, date/time, winner chip, team chips, scheme chip, stats strip for a processed replay | MET | `GameDetailPage.tsx:374–487` renders all elements |
| Duration, Turns, Max Damage, Kills computed correctly from turn data | MET | `GameDetailPage.tsx:326–334`; formulas match spec |
| Drawn game shows "Draw" chip with neutral (default) colour | MET | `GameDetailPage.tsx:411`: `color={replay.winner === 'Draw' ? 'default' : 'warning'}` |
| Scheme version chip absent when league has no version | MET | `GameDetailPage.tsx:435`: guarded by `league.version !== null` |
| Stats strip absent when no turn data | MET | `GameDetailPage.tsx:447`: `{hasTurns && (…)}` |
| Breadcrumb shows Leagues → League Name → Match #00x with correct links | MET | `GameDetailPage.tsx:349–371` |
| Turn-by-turn panel active by default; clicking Weapons switches panel | MET | `GameDetailPage.tsx:281,504–506` |
| One row per turn in order; all turns shown including no-weapons / no-damage turns | MET | `GameDetailPage.tsx:98–173` |
| Last weapon visually distinguished (bold) | MET | `GameDetailPage.tsx:130–134`: last weapon gets `sx={{ fontWeight: 700 }}` |
| Turns with no weapons show "—"; turns with no damage show "—" | MET | `GameDetailPage.tsx:113–116,141–144` |
| Weapons panel shows per-team entries with attributed damage sorted descending | MET | `GameDetailPage.tsx:197–229` |
| No log → hero card shown but both panels show empty-state message | MET | `turns === null` paths in `TurnByTurnPanel` and `WeaponsPanel`; `turns` is `null` (not `[]`) per DTO shape |
| Pending replay shows "processing" message with no hero card | MET | `GameDetailPage.tsx:341–344` |
| Non-existent or cross-league replay shows not-found message | MET | `GameDetailPage.tsx:292–295` (SPA); `LeaguesController.cs:85–92` (API) |
| Non-2xx from either API call shows error message | PARTIAL | `GameDetailPage.tsx:296–297`: error shown. However a 404 on `leagueRes` shows "Error: HTTP 404" rather than a "not found" message. This is an edge case (league must exist to navigate here) and is a pre-existing pattern in other pages. |
| Signed-out user redirected by `RequireAuth` | MET | `App.tsx:33–38`: route wrapped in `<RequireAuth>` |
| `GET /api/v1/leagues/{id}/replays/{replayId}` returns 401 for unauthenticated requests | MET | Inherited `[Authorize]` on `V1ApiController` |
| `GET /api/v1/leagues/{id}/replays/{replayId}` returns 404 for non-existent/cross-league replay | MET | `LeaguesController.cs:85–92` |
| Seeded replay shows populated stats strip and panels under `docker compose up` | MET | Seed added at `R__ReplaysTestData.sql:58–89`; log format matches parser expectations |
| `make web.build` and `make web.lint` both pass | MET | Both pass clean — verified locally |

## Scope

The diff exactly matches the plan's "Files to Create / Modify" table. All five planned file touches are present and no extra files were added. `learnings.md` accounts for all deviations (MUI v9 prop API changes, `slotProps` instead of `primaryTypographyProps`, `AddWormsArmageddonFilesServices` placement, Prettier re-run).

One deviation the `learnings.md` does not fully resolve: the plan's "simplest safe approach" says to call `AddWormsArmageddonFilesServices()` unconditionally in `Program.cs`. The implementation instead calls it only inside the `if (runGateway)` block. This is the source of **B1**.

## Blockers

#### B1 — `IReplayTextReader` not registered in distributed worker-only mode

- **File:** `src/Worms.Hub.Gateway/Program.cs:51`
- **Issue:** `AddWormsArmageddonFilesServices()` was removed from `AddWorkerServices()` and added only inside the `if (runGateway)` block. `Processor` (in `AddWorkerServices`) depends on `IReplayTextReader`. When the process runs in distributed worker-only mode (`HUB_DISTRIBUTED=true`, `HUB_WORKER=true`, `HUB_GATEWAY=false`), the gateway block is skipped and `IReplayTextReader` is never registered, causing DI resolution of `Processor` to fail at runtime.
- **Fix:** Move the `AddWormsArmageddonFilesServices()` call outside the `if (runGateway)` block to be unconditional — exactly as the plan's "simplest safe approach" described. The call is idempotent and safe in both modes.
- **Decision:** Accept

## Suggestions

#### S1 — Unit test coverage for `ReplayDetailDto.FromDomain`

- **File:** `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs`
- **Issue:** The `FromDomain` factory method contains non-trivial logic (turn indexing, null-vs-empty-list handling for turns when parsed has zero turns). The testing strategy doc notes that logic at gateway/storage layers has no dedicated test project, but recommends adding one when meaningful logic is present.
- **Fix:** Add a `Worms.Hub.Gateway.Tests` project (NUnit) with a `ReplayDetailDtoShould` test class covering: a replay with turns, a replay with a parsed-but-empty-turns list (should produce `null` for `Turns`), and a replay with no log (should produce `null` for `Turns`).
- **Decision:** Decline

#### S2 — `leagueRes` 404 not shown as "not found"

- **File:** `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx:292–297`
- **Issue:** Only `replayRes.status === 404` sets `notFound`. If `leagueRes` returns 404, the `!leagueRes.ok` branch throws and the user sees "Error: HTTP 404" instead of a "not found" message. This is a minor UX inconsistency but the case (valid route path, invalid league ID) is unlikely in practice.
- **Fix:** Add a check for `leagueRes.status === 404` before the `!leagueRes.ok` throw: `if (leagueRes.status === 404) { setNotFound(true); return; }`.
- **Decision:** Decline

## Nitpicks

#### N1 — Fully qualified `Worms.Armageddon.Files.Replays.ReplayResource` in controller body

- **File:** `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs:97`
- **Issue:** `Worms.Armageddon.Files.Replays.ReplayResource? parsed = null;` uses a fully qualified type name in the method body. The `ReplayDtos.cs` file already imports `using Worms.Armageddon.Files.Replays;`, and the controller has its own `using Worms.Armageddon.Files.Replays.Text;`. Adding `using Worms.Armageddon.Files.Replays;` to the controller would eliminate the qualification. The `learnings.md` notes this as intentional to avoid ambiguity, but there is no conflicting `ReplayResource` type in scope.
- **Fix:** Add `using Worms.Armageddon.Files.Replays;` to `LeaguesController.cs` and use `ReplayResource? parsed = null;`.
- **Decision:** Accept

## Tests

No new test code was added in this diff. This is consistent with the repo's current approach — the gateway and storage layers have no dedicated unit test projects and the testing strategy doc acknowledges the gap. The `ReplayDetailDto.FromDomain` logic (turn indexing, empty-turn null coercion) and the new controller action are both exercised only end-to-end under `docker compose`. See **S1** for the suggestion to add a test project.

The existing `Worms.Armageddon.Files.Tests/Replays/ReplayTextReaderShould.cs` covers the upstream parsing pipeline and was not modified.

## Recommended Actions

- **B1** — Accept — Moving the call outside the gateway block is a one-line fix and directly restores the behaviour the plan explicitly intended. Without it, a distributed worker-only deployment fails at startup.
- **S1** — Accept — The `FromDomain` logic is a clear unit-testable boundary; adding a minimal test class here would give early warning if the null-coercion or indexing logic is ever changed.
- **S2** — Decline — The case (user somehow navigates to `/leagues/invalid-id/replays/123`) is not reachable via normal UI navigation and the spec does not call it out explicitly. The fix is trivial but adds noise for a case with no realistic trigger.
- **N1** — Accept — The `learnings.md` rationale (avoid ambiguity) does not hold — there is no conflicting type. Removing the qualification aligns with the repo's standard style and the fix is trivial.
