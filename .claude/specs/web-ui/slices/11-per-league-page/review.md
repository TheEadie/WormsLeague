# Review — Per-League Page

## Verdict

The implementation satisfies all acceptance criteria. Both .NET builds (`Worms.Hub.Storage` and `Worms.Hub.Gateway`) exit clean with zero warnings at `--warnaserror`. `make web.build` and `make web.lint` both pass. There are no blockers. One unplanned migration file (`V0.3.1__SeedRedgateLeague.sql`) is present and necessary for the FK backfill to succeed but is unexplained in `learnings.md`. Two minor suggestions address the repeatable seed behaviour and an edge-case in the `not found` render branch.

---

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Signed-in user at `/leagues/redgate` sees league name, scheme version, and scheme download link | MET | `LeagueDetailPage.tsx:75-98` renders `league.name` in h4, scheme chip, and download link |
| Replay list shows one row per replay; processed rows show date/winner/teams, unprocessed rows show name and holding message | MET | `LeagueDetailPage.tsx:123-197` branches on `replay.processed` |
| Each replay row is a link to `/leagues/redgate/replays/{replayId}` | MET | `LeagueDetailPage.tsx:109-113` wraps rows in `<Link to={/leagues/${id}/replays/${replay.id}}>` |
| Signed-out user redirected to `/` by `RequireAuth` | MET | `App.tsx:24-29` wraps `<LeagueDetailPage />` in `<RequireAuth>` |
| `/leagues/nonexistent-league` shows "not found" message | MET | `LeagueDetailPage.tsx:49-51,70` sets `notFound` on 404; renders "League not found." |
| Loading indicator shown while API calls are in progress | MET | `LeagueDetailPage.tsx:67-69`: `CircularProgress` while all four state fields are null/false |
| Error message shown on network error or non-2xx response | MET | `LeagueDetailPage.tsx:62,71` catches errors and renders `color="error"` Typography |
| Under `docker compose up`, redgate league shows at least one processed replay | MET | `R__ReplaysTestData.sql` inserts a Processed row with date/winner/teams |
| `GET /api/v1/leagues/redgate/replays` returns HTTP 200 with non-empty array | MET | `LeaguesController.cs:47-58` + `R__ReplaysTestData.sql` seed |
| `GET /api/v1/leagues/{id}/replays` returns HTTP 200 with empty array for league with no replays | MET | `LeaguesController.cs:56-57`: `GetByLeagueId` returns empty list; `Ok(...)` returns 200 |
| `GET /api/v1/leagues/{id}/replays` returns HTTP 404 for unknown league id | MET | `LeaguesController.cs:50-53`: `GetById` returns null → `NotFound()` |
| `GET /api/v1/leagues/{id}/replays` returns HTTP 401 for unauthenticated requests | MET | Inherited from `V1ApiController`'s `[Authorize]` attribute |
| `make web.build` and `make web.lint` both pass | MET | Both commands exit 0 (verified in this review) |

---

## Scope

The diff matches the plan's Files to Create / Modify table with one addition:

**Unplanned new file: `src/database/migrations/V0.3.1__SeedRedgateLeague.sql`**

This file inserts `('redgate', 'Redgate')` into `public.leagues` as a versioned migration. It is not listed in the plan but is logically necessary: `V0.4.1__BackfillReplayLeagueFields.sql` sets `league_id = 'redgate'` on all existing replay rows, and `V0.4__AddReplayLeagueFields.sql` adds a FK constraint from `replays.league_id` to `leagues.id`. Without `redgate` pre-existing in `leagues`, the backfill would fail with a FK violation on any environment that has existing replay rows. The `learnings.md` does not mention this addition. The deviation is justified, but it deserves a note (see S1 below).

**Planned file `src/Worms.Hub.Gateway/Worker/Processor.cs` — sequencing deviation from plan** is explained in `learnings.md` and is the correct implementation order (parse then update). No concern.

**Planned file `src/Worms.Hub.Storage/Database/ReplaysRepository.cs` — `ReplaysController` positional constructor update** is noted in `learnings.md`. No concern.

All other planned files are present and match the described changes.

---

## Blockers

None.

---

## Suggestions

#### S1 — V0.3.1 migration not mentioned in learnings.md

- **File:** `src/database/migrations/V0.3.1__SeedRedgateLeague.sql`
- **Issue:** This unplanned versioned migration is necessary to satisfy the FK constraint added in V0.4, but it is absent from both the plan and learnings.md. Anyone reading the plan to understand what this slice changed will miss it.
- **Fix:** Add a brief note to `learnings.md` explaining why V0.3.1 was added (the FK backfill in V0.4.1 requires `redgate` to exist in `leagues`).
- **Decision:** Accept

#### S2 — `notFound` state does not suppress the loading indicator on replays-404 path

- **File:** `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx:67`
- **Issue:** When the leagues fetch returns 404, `setNotFound(true)` is called and `return` exits the `.then` handler, leaving `replays === null`. The loading-indicator condition is `league === null && replays === null && error === null && !notFound`, so `!notFound` correctly suppresses it — this actually works. However, if the replays fetch also returns a non-2xx in the same `Promise.all` response, the `if (!replaysRes.ok)` throw runs *before* `setNotFound` when `leagueRes.status === 404`. The current code checks `leagueRes.status === 404` first and returns early, so the replays error is silently discarded. This is acceptable behaviour (404 on league is the primary signal), but it is a subtle ordering dependency worth noting if the logic evolves.
- **Fix:** Consider adding a comment noting that the 404-guard short-circuits the replays error check intentionally.
- **Decision:** Decline

---

## Nitpicks

#### N1 — Seed file uses `ON CONFLICT DO NOTHING` without a conflict target

- **File:** `src/database/local-dev/R__ReplaysTestData.sql:9,19`
- **Issue:** The `replays` table has no unique constraint on `name` or `filename` — only a PK on the auto-generated `id` integer. `ON CONFLICT DO NOTHING` without a conflict target is valid PostgreSQL syntax (it silently ignores any constraint violation), but since `id` is generated and will never conflict on a fresh insert, the clause is a no-op. The repeatable migration will insert additional rows on every Flyway run rather than being idempotent. The other seed files (`R__GamesTestData.sql`) use plain `INSERT` with the same behaviour, so this is consistent with the existing pattern, but the `ON CONFLICT` adds false confidence of idempotency.
- **Fix:** Either remove `ON CONFLICT DO NOTHING` to match the other seed files, or prefix with `DELETE FROM public.replays;` (consistent with how `R__LeaguesTestData.sql` handles idempotency).
- **Decision:** Accept

#### N2 — `ReplayDb.FullLog` was previously non-nullable; now nullable

- **File:** `src/Worms.Hub.Storage/Database/ReplaysRepository.cs:93`
- **Issue:** `ReplayDb` previously declared `FullLog` as `string FullLog` (non-nullable). The diff changes it to `string? FullLog`. This is the correct change (the `full_log` column is nullable in the DB schema added in V0.2.2), but it was silently incorrect before — Dapper would have mapped `NULL` to an empty string or default, not a nullable. The fix is correct but it is a silent behaviour change in `GetAll()` — callers that previously saw `""` for unprocessed replays will now see `null`.
- **Fix:** No code change needed; the new behaviour is correct. Noting it here because it is a pre-existing latent bug that this slice incidentally fixes.
- **Decision:** Accept

---

## Tests

No new unit or integration tests were added for this slice. This is consistent with the testing strategy: the gateway, storage, and web layers do not have dedicated unit-test projects, and the testing strategy notes that "behaviour at those layers is exercised indirectly via the integration tier." The new `GetByLeagueId` method, `GetReplays` action, and `LeagueDetailPage` component are all covered by the strategy's expectation that integration tests (against a real Postgres) would exercise the data path.

No padding tests were added or removed.

---

## Recommended Actions

- **S1** — Accept — A one-line note in `learnings.md` costs nothing and makes the slice history complete.
- **S2** — Decline — The behaviour is correct; a comment is optional and the spec does not ask for it.
- **N1** — Accept — Removing the spurious `ON CONFLICT DO NOTHING` and matching the `DELETE FROM` pattern of other seed files makes the intent clear and the file consistent.
- **N2** — Accept — No code change needed; this is informational only.
