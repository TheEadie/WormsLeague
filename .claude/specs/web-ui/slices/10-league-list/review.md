# Review — League List

## Verdict

The implementation satisfies all acceptance criteria. Both build targets pass clean with no warnings, ESLint and TypeScript type-check are clean, and Prettier reports no formatting issues. The one meaningful deviation from the plan — making `LeaguesRepository` `public sealed` instead of `internal sealed` — is correctly explained in `learnings.md` and is the right call given cross-assembly injection. There are no blockers. A couple of minor observations are noted below as suggestions and nitpicks.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Signed-in user at `/leagues` sees a card per league (name, id, scheme version) | MET | `LeagueListPage.tsx:62–73` renders name, id, and `v{version}` per league |
| Each card links to `/leagues/{id}` | MET | `LeagueListPage.tsx:57–59` wraps each card in `<Link to={/leagues/${league.id}}>` |
| Signed-out user at `/leagues` is redirected to `/` by `RequireAuth` | MET | `App.tsx:17–22` wraps `/leagues` in `<RequireAuth>` |
| After OIDC callback, user lands on `/leagues` | MET | `CallbackPage.tsx:14` navigates to `/leagues` |
| While API call is in progress, loading indicator is visible | MET | `LeagueListPage.tsx:41` renders `<CircularProgress />` while `leagues === null && error === null` |
| If API call fails, error message is displayed | MET | `LeagueListPage.tsx:42–44` renders error Typography when `error !== null` |
| `GET /api/v1/leagues` returns HTTP 200 with array (populated or empty) | MET | `LeaguesController.cs:10–22` `GetAll()` returns `Ok(results)` unconditionally |
| `GET /api/v1/leagues` returns HTTP 401 for unauthenticated requests | MET | `[Authorize]` is inherited from `V1ApiController`; no change required |
| `GET /api/v1/leagues/{id}` returns HTTP 404 when id absent from DB | MET | `LeaguesController.cs:28–31` returns `NotFound()` when `dbLeague is null` |
| `GET /api/v1/leagues/{id}` returns HTTP 401 for unauthenticated requests | MET | `[Authorize]` inherited; no change required |
| "redgate" league appears under `docker compose up` | MET | `R__LeaguesTestData.sql:1–2` seeds `('redgate', 'Redgate')` |
| `make web.build` and `make web.lint` pass | MET | Both confirmed clean (ESLint, tsc, Prettier all exit 0) |

## Scope

All files match the plan's "Files to Create / Modify" table. Summary:

- `src/database/migrations/V0.3__AddLeagues.sql` — created, matches plan exactly.
- `src/database/local-dev/R__LeaguesTestData.sql` — created, matches plan.
- `src/Worms.Hub.Storage/Database/LeaguesRepository.cs` — created; `public sealed` instead of `internal sealed`, deviation explained and justified in `learnings.md`.
- `src/Worms.Hub.Storage/ServiceRegistration.cs` — `AddScoped<LeaguesRepository>()` added.
- `src/Worms.Hub.Gateway/API/Controllers/LeaguesController.cs` — list action added, `GetById` 404 guard added.
- `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx` — created; uses `<Link>` wrapper approach (not `CardActionArea component={Link}`) as the plan's own caveat recommended.
- `src/Worms.Hub.Web/src/pages/CallbackPage.tsx` — redirect updated to `/leagues`.
- `src/Worms.Hub.Web/src/App.tsx` — `/authenticated` removed; `/leagues` and `/leagues/:id` added.
- `src/Worms.Hub.Web/src/pages/AuthenticatedPage.tsx` — deleted.

No out-of-scope files were touched. The `.claude/specs/web-ui/plan.md` modification in the working tree is a workflow artefact (marking the slice complete) — not feature code.

## Blockers

None.

## Suggestions

#### S1 — `LeaguesRepository` missing `[PublicAPI]` on the class itself

- **File:** `src/Worms.Hub.Storage/Database/LeaguesRepository.cs:8`
- **Issue:** `GamesRepository` and `ReplaysRepository` are `internal sealed` but their DB-record types carry `[PublicAPI]` to document that Dapper and/or DI wire them up reflectively. `LeaguesRepository` is `public sealed` (so the annotation is less critical), but following the same pattern would make the intent explicit and consistent.
- **Fix:** Add `[PublicAPI]` above `public sealed class LeaguesRepository`.
- **Decision:** Accept

#### S2 — `LeagueRecord` name deviates from `XxxDb` convention

- **File:** `src/Worms.Hub.Storage/Database/LeaguesRepository.cs:27`
- **Issue:** The hub-storage component doc ("Adding a new domain object", step 2) specifies the Dapper mapping type be named `XxxDb`. The equivalent types in the same file are `GamesDb` and `ReplayDb`. Naming it `LeagueRecord` instead of `LeagueDb` breaks the naming convention.
- **Fix:** Rename `LeagueRecord` to `LeagueDb` in `LeaguesRepository.cs` and update the corresponding reference in `LeaguesController.cs` (only used as the return type of `GetAll()` / `GetById()`, both within the controller's local scope).
- **Decision:** Accept

## Nitpicks

#### N1 — `CardActionArea` import left unused

- **File:** `src/Worms.Hub.Web/src/pages/LeagueListPage.tsx`
- **Issue:** The plan initially considered using `CardActionArea` but the implementation correctly switched to the `<Link>` wrapper approach. The import for `CardActionArea` from `@mui/material/CardActionArea` is absent from the actual file — this nitpick was pre-empted. (No action required; confirmed line 8 only imports `CardContent`.) Nothing to fix here.
- **Decision:** Decline

## Tests

No new tests were added. This is appropriate under the project's testing strategy:

- The new `LeaguesRepository` methods hit a real Postgres database. The strategy doc explicitly discourages mocking repositories and prefers integration tests against a real DB. No unit-test project exists for `Worms.Hub.Storage` and adding one would require mocking so much of the DB stack that it would test the mock rather than the code.
- The new `GET /api/v1/leagues` and updated `GET /api/v1/leagues/{id}` endpoints are similarly covered by the integration tier (manual `docker compose` verification against real Postgres), consistent with how the other Gateway endpoints are handled.
- The new `LeagueListPage.tsx` is a data-fetching page. A unit test would require mocking `useAuth`, `fetch`, and the OIDC context — the existing SPA test precedent (slice 09, `RequireAuth.test.tsx`) tests component-level logic; a simple data-fetching effect is not meaningfully different from the placeholder `AuthenticatedPage` that was just removed, which also had no test.

There is no coverage gap that violates the testing strategy. No padding tests to remove.

## Recommended Actions

- **S1** — Accept — Keeps annotation style consistent with sibling repositories; one-line change, zero risk.
- **S2** — Accept — `LeagueDb` matches the component doc convention and aligns with `GamesDb` / `ReplayDb` in the same file; rename is local to `LeaguesRepository.cs` with no external consumers yet.
- **N1** — N/A — Issue does not exist in the implemented code; no action needed.
