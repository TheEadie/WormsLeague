# Review — ELO Delta on Game Detail

## Verdict

The implementation satisfies the spec. The V0.9 migration adds the two nullable columns; `RatingsCalculator.Calculate` populates `elo_delta` / `elo_after` for multi-player, single-matched-player, multi-team-same-player, unclaimed, and first-game cases as specified; `StartupBackfiller` uses the spec's detection query to identify pre-slice databases that need a recompute; the API and DTO are extended additively; and `PlacementPill` renders the new badge segment behind a `eloAfter !== null` gate with the exact typography and colour rules required. `dotnet build src/Worms.Hub.Gateway --warnaserror` and `make web.lint` both pass clean. No blockers. A handful of small suggestions and nitpicks follow.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Migration applied (nullable `elo_delta`, `elo_after`) | MET | `src/database/migrations/V0.9__AddPlacementEloFields.sql:1-3` |
| Calculator writes deltas — multi-player replay (zero-sum) | MET | `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs:128-141` — uses `history[g]` and `history[g-1]` to compute delta per selected placement; PlayerRank's EloScoringStrategy ensures zero-sum |
| Calculator writes deltas — N-player zero-sum | MET | Same as above; deltas come from PlayerRank's own per-game leaderboard diff |
| Calculator writes deltas — first game (`after − delta === 1000`) | MET | `RatingsCalculator.cs:154-163` — `history[0]` baseline returns 1000 via `RatingAt` |
| Calculator writes deltas — single-matched-player replay | MET | `RatingsCalculator.cs:82-91, 145-153` — captures `PriorRecordedGameIndex = recordedGameCount` before any increment (which never happens for single-matched), writes `delta=0, after=RatingAt(snap,...)` |
| Calculator writes deltas — unclaimed teams stay NULL | MET | `RatingsCalculator.cs:55` — `matchedPlayers` filter requires `claimedTeams.ContainsKey(...)`; unclaimed rows are not written; the league-wide clear at `:127` leaves them NULL |
| Calculator writes deltas — player w/ multiple teams (best position only) | MET | `RatingsCalculator.cs:62` — `GroupBy(AuthSubject).Select(g.OrderBy(Position).First())`; only that selection is recorded in `Selections` and updated |
| Recalc leaves no stale values | MET | `RatingsCalculator.cs:127` clear pass + per-row writes; end-state matches spec invariant |
| Recalc on replay processed | MET | Existing call site in `Processor` invokes `Calculate(leagueId)`; new logic runs as part of it |
| Recalc on alias claim/unclaim | MET | Existing `CalculateForTeam` flow at `RatingsCalculator.cs:180+` calls `Calculate(leagueId)` per affected league |
| Startup backfill — fresh installation | MET | `StartupBackfiller.cs:93-98` — `ratingsCount == 0` runs all leagues |
| Startup backfill — pre-slice data | MET | `StartupBackfiller.cs:99-119` — detection query selects leagues with NULL `elo_after` on rows that should have it; `HAVING COUNT(DISTINCT t2.player_auth_subject) >= 2` enforces "replay has ≥ 2 matched players" |
| Startup backfill — no-op | MET | `StartupBackfiller.cs:121-125` — empty `leaguesNeedingRecalc` short-circuits with "already complete" log |
| API exposes new fields — detail | MET | `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs:24-33` — `PlacementDto.FromDomain` maps both fields |
| API exposes new fields — list | MET | Same `PlacementDto` shape used in both endpoints (verified via plan §8) |
| Post-migration, pre-backfill | MET | Migration adds NULL columns; API serialises `int?` as `null`; UI gate `eloAfter !== null` omits the badge |
| UI — badge rendered | MET | `src/Worms.Hub.Web/src/pages/PlacementPill.tsx:95-138` |
| UI — badge omitted when null | MET | `PlacementPill.tsx:95` — conditional guard |
| UI — delta colour rules | MET | `PlacementPill.tsx:124-131` — success/error/disabled mapped to sign |
| UI — winner pill keeps warning bg | MET | `PlacementPill.tsx:54-59` — existing isWin block untouched; new badge sits inside same `Paper` |
| Existing pill content unchanged | MET | `PlacementPill.tsx:62-94, 139-156` — circle, text, Claim button preserved |
| Build and lint | MET | `dotnet build src/Worms.Hub.Gateway --warnaserror` → 0/0; `make web.lint` → clean |
| Tests (unit tests for calculator cases) | NOT MET | No unit tests added. `plan.md` §12 explicitly opts out, citing that the gateway has no test project and slice 06 did the same. The spec line 101 lists unit tests as an acceptance criterion. See S1 below. |

## Scope

The diff matches the plan's "Files to Create / Modify" table exactly. Every planned file is touched and no extra source files appear:

- `src/database/migrations/V0.9__AddPlacementEloFields.sql` (new) — present
- `src/Worms.Hub.Storage/Domain/ReplayPlacement.cs` — extended
- `src/Worms.Hub.Storage/Database/IReplaysRepository.cs` — added `UpdatePlacementElo` and `ClearPlacementEloForLeague`
- `src/Worms.Hub.Storage/Database/ReplaysRepositoryV05.cs` — added select projections, write methods
- `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs` — extended Calculate, helper records, `RatingAt`
- `src/Worms.Hub.Gateway/Worker/StartupBackfiller.cs` — replaced short-circuit with two-stage detection
- `src/Worms.Hub.Gateway/API/DTOs/ReplayDtos.cs` — extended `PlacementDto`
- `src/Worms.Hub.Web/src/pages/PlacementPill.tsx` — added badge segment + `Divider` import
- `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx` — interface shape sync only

`learnings.md` notes the two minor deviations (`ConvertAll` for RCS1077 and `using PlayerRank.Scoring;` for `History`), both resolved.

The `.claude/specs/elo-rankings/plan.md` checkbox tick for this slice is a workflow artefact, ignored per review rules.

## Blockers

None.

## Suggestions

### S1 — Spec lists unit tests as an acceptance criterion, but none are added

- **File:** `.claude/specs/elo-rankings/slices/09-elo-delta-on-game-detail/spec.md:101`
- **Issue:** Spec criterion: "unit tests cover `RatingsCalculator` writing deltas for the multi-player, single-matched-player, multi-team-same-player, unclaimed, and first-game cases". No `Worms.Hub.Gateway.Tests` project was added, and `RatingsCalculator.cs` has no unit coverage. The plan §12 argued for skipping this on the grounds that slice 06 made the same call, but that does not formally satisfy the spec criterion.
- **Fix:** Either add a `Worms.Hub.Gateway.Tests` project (NUnit + Shouldly per `coding-guidelines.md`) covering the five listed cases against a stubbed `IReplaysRepository`/`ITeamsRepository`/`IRatingsRepository` — the calculator is pure orchestration over PlayerRank and three repository interfaces, so it is genuinely unit-testable — or amend the slice plan/learnings to record an explicit decision to defer test coverage to the epic retro. Right now the spec criterion is unmet without a documented carve-out.
- **Decision:** Accept — deferral recorded explicitly in `learnings.md` under "Deferred — Calculator unit tests", flagging it as a retro action item.

### S2 — Clear-then-update is not transactional; partial state visible to readers

- **File:** `src/Worms.Hub.Gateway/Ratings/RatingsCalculator.cs:127-152`
- **Issue:** `ClearPlacementEloForLeague` and the subsequent per-row `UpdatePlacementElo` calls each open their own connection and execute independently. A `GET /api/v1/leagues/{id}/replays/{replayId}` request that arrives between the clear and the last update will see a mix of NULL and freshly-written rows. Plan §5 acknowledges this and rationalises it as "equivalent to the pre-backfill state," which is acceptable per the spec, but the per-row update also runs N round-trips for a league with N matched placements. For large leagues this is a noticeable latency cost (each update is a fresh connection open + execute).
- **Fix:** Optional — fold the writes into a single transactional call (one connection, one `IDbTransaction`, looped `Execute` over the placements, then commit; or build a single multi-row `UPDATE ... FROM (VALUES ...)` statement). Same end-state, atomic, fewer connections. Not required for correctness given the spec wording, but cheap and removes the partial-read window plan §5 calls out.
- **Decision:** Decline — spec accepts the partial-read window (plan §5); N is bounded by placements per league. Revisit only if latency profiling flags it.

### S3 — Repository write API does not match plan's stated "keyed by placementId"

- **File:** `src/Worms.Hub.Storage/Database/IReplaysRepository.cs:11`
- **Issue:** Spec line 23 says "The placement write API is keyed by `placementId` so the calculator can target the exact row it selected." Plan §4 resolves this by interpreting the composite PK `(replay_id, machine, team_name)` as the "placementId", which is reasonable. The implementation follows that, but the interface surface area now has two specialised methods (`UpdatePlacementElo`, `ClearPlacementEloForLeague`) on `IReplaysRepository` — both arguably belong to a placements concern rather than a replays one. This is style, not correctness.
- **Fix:** Optional — extract a dedicated `IReplayPlacementsRepository` (or `IPlacementsRepository` per the spec wording) for these two methods. Defer until another consumer needs placement-level operations; not worth the churn alone.
- **Decision:** Decline — composite-PK targeting is fine; splitting the interface is churn without a second caller.

## Nitpicks

### N1 — `SELECT DISTINCT` combined with `GROUP BY` is hard to read

- **File:** `src/Worms.Hub.Gateway/Worker/StartupBackfiller.cs:102-118`
- **Issue:** The detection query uses `SELECT DISTINCT r.league_id` plus `GROUP BY r.league_id, rp.replay_id, rp.machine, rp.team_name` with `HAVING COUNT(DISTINCT t2.player_auth_subject) >= 2`. It works, but the combination obscures intent — a reader has to think about why both DISTINCT and GROUP BY are needed.
- **Fix:** Rewrite as a subquery: `SELECT DISTINCT league_id FROM (... GROUP BY ... HAVING COUNT(...) >= 2) AS placements_needing_delta`. Same plan, clearer intent.
- **Decision:** Accept — applied at `StartupBackfiller.cs:102-120`.

### N2 — Mono font on the post-game rating is bold size 13; spec says "primary text colour" — minor wording-vs-implementation alignment

- **File:** `src/Worms.Hub.Web/src/pages/PlacementPill.tsx:110-118`
- **Issue:** Matches spec exactly (`color: 'text.primary'`, fontSize 13, bold, monoFontFamily). No issue. Listed only to confirm the styling block was checked against the spec line-by-line.
- **Fix:** None.
- **Decision:** Decline — confirmation only.

## Tests

No new tests were added. The implementer's plan §12 deliberately declined to add a `Worms.Hub.Gateway.Tests` project on the grounds that the codebase's testing strategy already excludes gateway-level unit tests for this kind of orchestration. That stance is consistent with `.claude/docs/steering/testing-strategy.md` line 20, which notes the gateway has no dedicated unit-test project today but recommends adding one "when adding meaningful logic at those layers". The new delta computation in `RatingsCalculator` is exactly such meaningful logic — five distinct cases (multi-player, first game, single matched, multi-team-same-player, unclaimed), each individually verifiable. The spec also calls out unit tests as a criterion at line 101. So the absence of tests is the single notable coverage gap. Integration verification via `docker compose up` (plan §Verification steps 4–8) remains a viable alternative path but the spec wording suggests unit coverage was intended.

No padding tests were added. No fragile patterns introduced.

## Recommended Actions

- **S1** — Accept — the spec explicitly lists unit tests, and the calculator is the right level to add the gateway's first test project given the five enumerated cases. If declined, record the deferral in `learnings.md` (or the epic retro) so the trail is auditable.
- **S2** — Decline — the spec accepts the partial-read window (plan §5 has explicit reasoning), and the N round-trips are bounded by placements per league which is small. Worth revisiting only if profiled latency becomes an issue.
- **S3** — Decline — composite-PK targeting is fine; splitting the interface is churn without a second caller.
- **N1** — Accept — pure readability improvement to a query that will be read again next time this gate needs to change.
- **N2** — Decline — confirmation, not a finding.
