# Retrospective — elo-rankings

## From slice files

### L1 — Plans do not anticipate Roslynator/Roslyn warning-as-error rules

- **Pattern:** Implementers repeatedly had to deviate from the plan to satisfy analyser rules promoted to errors by `--warnaserror`: RCS1124 (inline local variable), RCS1146 (conditional access), CA1852 (seal internal types), CA1305 (`CultureInfo.InvariantCulture` on integer formatting), CA1031 (catch `Exception` requires `[SuppressMessage]` on the enclosing method, not the catch clause), CA1062 (`ArgumentNullException.ThrowIfNull` on public record parameters), and RCS1077 (`ConvertAll` instead of `Select(...).ToList()` on a `List<T>`). Each rule surfaced mid-flight even though the codebase has hit them before.
- **Evidence:** slices/02-placement-persistence/learnings.md, slices/03-placement-display/learnings.md, slices/06-elo-rankings/learnings.md (CS0104 + cultureinfo-style rules), slices/07-elo-on-alias-changes/learnings.md, slices/09-elo-delta-on-game-detail/learnings.md, slices/10-slack-post-leaderboard/learnings.md
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add a checklist of the analyser rules that the codebase routinely trips (RCS1124, RCS1146, RCS1077, CA1031, CA1062, CA1305, CA1852) with the canonical fix pattern for each, so planners know to bake the compliant form into snippets rather than the implementer rediscovering them.

### L2 — Plans omit prerequisite `using` directives and namespace conflicts

- **Pattern:** Code snippets in plans regularly compile in isolation but fail in the codebase because a required `using` is missing (`Microsoft.Extensions.DependencyInjection.Extensions` for `TryAddScoped`, `PlayerRank.Scoring` for `History`) or because a referenced type collides with a domain type (`PlayerRank.League` vs `Worms.Hub.Storage.Domain.League`). Implementers fix it by adding the directive or fully-qualifying.
- **Evidence:** slices/02-placement-persistence/learnings.md, slices/06-elo-rankings/learnings.md, slices/09-elo-delta-on-game-detail/learnings.md, slices/11-remove-feature-flags/learnings.md
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — document the namespace collisions known to exist (PlayerRank vs domain types), and state that any `TryAdd*` call requires `Microsoft.Extensions.DependencyInjection.Extensions` (it is not part of the implicit usings on `Microsoft.NET.Sdk.Web`).

### L3 — Meaningful Gateway logic ships without unit tests despite the spec asking for them

- **Pattern:** `RatingsCalculator` (5+ orchestration cases), `LeaderboardFormatter` (tied-rank, sign, zero suppression, escaping), and `BuildLeaderboard` in `Processor` were all added without tests. Each slice rationalised the skip by pointing at the absence of a `Worms.Hub.Gateway.Tests` project — even when the spec explicitly listed unit tests as an acceptance criterion (slice 09).
- **Evidence:** slices/06-elo-rankings/review.md (Tests section), slices/09-elo-delta-on-game-detail/review.md (S1) and learnings.md ("Deferred — Calculator unit tests"), slices/10-slack-post-leaderboard/review.md (S1)
- **Where to fix it:** `.claude/docs/steering/testing-strategy.md` — make explicit that when a slice introduces non-trivial logic into the Gateway (e.g. ratings, formatting, ranking) the slice must create `Worms.Hub.Gateway.Tests` rather than defer, and that an acceptance criterion calling for unit tests cannot be discharged by pointing at the missing test project.

### L4 — Plans omit prerequisite local environment steps for the Web project

- **Pattern:** `make web.lint` and `npm run build` both fail with `ERR_MODULE_NOT_FOUND` or missing-type errors when `node_modules` is not populated in the worktree. Plans listed the lint/build commands as verification but did not state that `npm ci` / `npm install` inside `src/Worms.Hub.Web` is a prerequisite.
- **Evidence:** slices/05-alias-claiming-replay-detail-inline/learnings.md, slices/11-remove-feature-flags/learnings.md
- **Where to fix it:** `.claude/docs/components/web.md` — add an explicit "before running `make web.lint` or `npm run build` in a fresh worktree, run `npm ci` in `src/Worms.Hub.Web`" note so planners include it as the first verification step.

### L5 — Plans miss the cascade when introducing `await` into a previously-synchronous method

- **Pattern:** Adding a single `await` (e.g. a feature flag check) requires the method's return type to become `Task<...>` and the `async` keyword to be added — a mechanical but easy-to-miss cascade. Plans showed the new `await` but not the signature change.
- **Evidence:** slices/03-placement-display/learnings.md (`GetReplays`/`GetReplay`), slices/07-elo-on-alias-changes/learnings.md (`AddGatewayServices` expression-bodied → block body because `TryAddScoped` is void)
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — under the existing async/DI guidance, add a note that any plan snippet introducing `await` or a void DI call into an existing expression-bodied or sync method must also restate the changed signature/body, so reviewers catch the mismatch in the plan.

### L6 — Plans assume a record signature change has a single construction site when it does not

- **Pattern:** Slice 01's plan asserted that only `ReplayResourceBuilder.Build()` constructs `ReplayResource`, so adding a positional record parameter looked self-contained. The CLI's `LocalReplayRetriever` actually constructs the record directly too, and the implementer discovered the extra site only when the build broke.
- **Evidence:** slices/01-parser-placement-extraction/learnings.md, slices/01-parser-placement-extraction/review.md (Scope table)
- **Where to fix it:** `.claude/docs/components/armageddon-files.md` — record that `ReplayResource` (and any shared positional record exposed to the CLI) has consumers outside the `Worms.Armageddon.Files` assembly, and require plans that change such records to grep across `src/Worms.Cli*` and `src/Worms.Hub*` for direct constructor calls before declaring the change self-contained.

### L7 — React keys repeatedly use display strings rather than stable identifiers

- **Pattern:** Three independent slices wired up new keyed lists using the rendered label or the player's display name as the React `key`, even though stable identifiers (machine+team, array index for a server-ordered list, auth subject) were available. Reviewers flagged the same issue each time.
- **Evidence:** slices/03-placement-display/review.md (S1), slices/06-elo-rankings/review.md (S1), slices/08-elo-leaderboard-on-league-cards/review.md (N1)
- **Where to fix it:** `.claude/docs/components/web.md` — add a "React keys" guideline that names the preferred stable-key choices for the recurring list shapes in this codebase (placement rows, standings rows, team rows) and explicitly disallows keying by display name or label string.

### L8 — Component docs go stale alongside the code changes that should update them

- **Pattern:** When a slice changes a domain model or a public method signature, the corresponding diagram or prose in the component doc is left referring to the old shape (`ReplayResource` without `Placements`, `AnnounceGameComplete(winner)` without the placements argument). Reviewers had to call this out as a nitpick on multiple slices.
- **Evidence:** slices/01-parser-placement-extraction/review.md (N1), slices/03-placement-display/review.md (N1)
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add an explicit "any change to a public method signature or to a shape illustrated in a component doc must update the doc in the same slice" rule, so the doc edit is treated as part of the plan's file list rather than a follow-up.

### L9 — `TryAddScoped` convention not applied consistently across DI registration methods

- **Pattern:** `AddGatewayServices` was updated to use `TryAddScoped` to support monolith mode where both gateway and worker register-services run, but `AddWorkerServices` continued to register the same concrete type via plain `AddScoped`, producing a silent double-registration. The convention is already documented but new registrations skipped it.
- **Evidence:** slices/07-elo-on-alias-changes/review.md (S1)
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — strengthen the existing `TryAddScoped`/`TryAddSingleton` note into a hard rule for any type registered from more than one extension method, with a worked example showing both `AddGatewayServices` and `AddWorkerServices` using the `Try*` form.

### L10 — Acceptance criteria are written in absolute terms the algorithm cannot meet

- **Pattern:** Slice 01's spec required "excess kills have no effect on any team's recorded position", but the algorithm infers `wormsPerTeam` from observed kills and so excess kills demonstrably can change other teams' positions. The implementer landed a test that asserts the actual (limited) behaviour and labelled it with the absolute name, which the reviewer flagged as misleading.
- **Evidence:** slices/01-parser-placement-extraction/review.md (S2, Verdict), slices/01-parser-placement-extraction/learnings.md ("Excess kills inflate `wormsPerTeam`")
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add planning guidance that acceptance criteria using absolute language ("no effect", "always", "never") must either be provably enforceable by the chosen algorithm or be rewritten to describe the algorithm's actual contract; tests must be named after what they assert, not the aspirational behaviour.

### L11 — Prettier reformats hand-pasted TSX from plan snippets

- **Pattern:** TSX code snippets in plans round-tripped through Prettier produce diffs (indent width, trailing commas, line-break placement around `as const`). Implementers had to run `npx prettier --write` after pasting; one slice noted Prettier reflowed the snippet without manual intervention.
- **Evidence:** slices/03-placement-display/learnings.md, slices/08-elo-leaderboard-on-league-cards/learnings.md
- **Where to fix it:** `.claude/docs/components/web.md` — add a one-liner that any plan editing `.tsx`/`.ts` must end with `npx prettier --write src` before `make web.lint`, and that planners should not hand-format TSX snippets to match Prettier — let the formatter own it.

### L12 — Migration- and infrastructure-version bookkeeping not encoded in the slice template

- **Pattern:** Several slices added DB migrations (V0.5, V0.7, V0.8, V0.9) but did not update `deployment/Worms.Hub.Infrastructure/Pulumi.yaml`'s database version; the team's convention is to bump it in a separate PR. The convention is real but undocumented, so each new slice has to rediscover it.
- **Evidence:** slices/02-placement-persistence/review.md (Scope note on `Pulumi.yaml`)
- **Where to fix it:** `.claude/docs/components/infrastructure.md` — document the rule that database-migration slices do not bump `Pulumi.yaml`'s database version inline; the version bump lands in a follow-up infra PR. That way the plan template can stop listing the file as a candidate edit and reviewers don't have to re-explain the convention.

## From PR #1338

### P1338.1 — Slices add hardcoded seed data for state that runtime backfillers will produce

- **Pattern:** Slice 06 both (a) added a `StartupBackfiller` that recomputes placements and ratings on first run and (b) hardcoded equivalent placement/rating rows in `src/database/local-dev/` seed data. The two sources drift the moment the calculator changes. A follow-up commit removed the seed-data duplication and noted it was "redundant and a source of drift". `review.md` verified acceptance criteria but did not check whether the new seed-data rows duplicated what the new runtime code already produces.
- **Evidence:** PR #1338 follow-up commit a077b97 ("fix(seed): remove hardcoded placements and ratings from sample data")
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` (or `.claude/docs/steering/coding-guidelines.md`) — state the rule that derived/computed state (placements, ratings, future leaderboard snapshots) must not be hardcoded in local-dev seed data when a backfiller or processor already generates it; seed only the source-of-truth inputs (replays, teams, claims) and let the runtime code populate the derived rows on first run.

### P1338.2 — Seed-data replay logs are not validated against the parser they feed

- **Pattern:** Local-dev replay 3 had Delta killing Gamma twice, but Gamma was already eliminated after the first kill — an impossible event log. The placement algorithm reacted by inferring `wormsPerTeam = 2`, which left Alpha/Beta tied at 1st with Delta instead of the intended Delta 1st / Beta 2nd / Alpha 3rd / Gamma 4th. The defect existed in seed data but only surfaced once ELO consumed placements and produced visibly wrong standings. `review.md` verified that ELO ordering came from placement data but did not trace the seed kill log back through the parser to confirm the seed produced the intended placements.
- **Evidence:** PR #1338 follow-up commit f80467d ("fix(seed): correct replay 3 kill order to produce intended placements")
- **Where to fix it:** `.claude/docs/components/armageddon-files.md` — document that fabricated replay logs (used in seed data, fixtures, or tests) must obey the elimination invariant (a team cannot be killed after losing its last worm) because the parser infers `wormsPerTeam` from observed kills; add a "before committing a fabricated replay log, run it through `ReplayResourceBuilder` and confirm the resulting placements match the intent" rule.

## From PR #1339

### P1339.1 — Specs describe data, not visual treatment, so UI polish ships as a follow-up PR

- **Pattern:** Slice specs for new tables/lists (placement chips, ELO standings) detail what data appears in each column but not the visual treatment that makes the surface match the rest of the app — medal circles on rank positions, emphasis styling on the headline metric (e.g. rating in `primary.main` like Most Damage), condensed/secondary header copy (e.g. `Games Played` → `Played` to match the `Length` column). PR #1339 had to land all three retrofits to the standings table because slice 06 only specified columns and ordering. The result is an extra polish PR per surface and a brief window where the new table looks inconsistent with the existing ones.
- **Evidence:** PR #1339 (polish follow-up — medal circles, ELO→Rating with prominent styling, Games Played → Played); slices/06-elo-rankings/spec.md (no visual reference to the replays table conventions it should mirror)
- **Where to fix it:** `.claude/docs/components/web.md` — add a "Table conventions" section naming the existing styling vocabulary (medal circles for rank, `primary.main` + heavier weight for the headline metric, secondary monospace for de-emphasised numeric columns, short column headers) and require any new table-or-list slice spec to either reuse that vocabulary or call out a conscious deviation, so the polish is captured in the original slice rather than deferred.

### P1339.2 — Seed data uses generic placeholders instead of Worms-themed names

- **Pattern:** New seed rows added during slices used generic placeholders (`Team Alpha/Beta/Gamma/Delta`, `Alpha Player`, `Other Player`) that read as scaffolding rather than as believable example data. PR #1339 had to retroactively replace them with themed names (Blitz Brigade, Banana Boys, Holy Rollers, Mad Bombers; BazookaJoe, Concrete Donkey). The fix is trivial but recurring — each slice that touches `src/database/local-dev/` reintroduces the placeholder pattern.
- **Evidence:** PR #1339 commit `a28f9a6c` ("replace placeholder names with Worms-themed ones")
- **Where to fix it:** `.claude/docs/components/hub-storage.md` (or wherever local-dev seed data is described) — state that seed rows in `src/database/local-dev/` must use Worms-themed names (teams: weapon/animal puns; players: in-game character or weapon references) and list a few canonical examples, so planners pick a themed name rather than `Team Alpha` when extending the seed set.

## From PR #1334

### P1334.1 — Slices expand in scope after the review.md and the new work is never reviewed

- **Pattern:** Slice 05's plan and review.md scoped the change as frontend-only (one file: `GameDetailPage.tsx`). After `review.md` was written, seven additional commits landed in the same PR — adding a new `PlayerName` field to `PlacementDto` with a backend LEFT JOIN, a refetch-after-claim bug fix, league-page enrichment, local-dev seed data, and a nullable placeholder on `ReplayPlacement.DisplayName` in `Processor.cs` that the human reviewer explicitly flagged as a "smell". None of this work was specced, planned, or reviewed; the slice-complete review.md predates it.
- **Evidence:** PR #1334 review comment by TheEadie on `Processor.cs:79` (`DisplayName = null` "is a bit of a smell. Need to rethink this at a later date."), follow-up commits `ea2b53cb` (join player name onto placements), `8d12016b` (player + team name on pills), `e15c4704` (hide claim buttons), `2566d10d` (league page enrichment), `9687f974` (seed data), `6c987408` (refetch after claim) — all after the review.md commit `1cdfaf59`.
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` (or the slice-workflow steering doc) — make explicit that once `review.md` is written and the slice is marked complete, additional scope landing in the same PR must either (a) trigger a new spec/plan/review cycle within the PR or (b) move to a follow-up slice. A bare DTO field added as `null` because "we'll fix it later" should be a spec, not a drive-by.

## From PR #1325

### P1325.1 — Batch-mode (`WORMS_BATCH=true`) path bypasses `IHostedService` registrations

- **Pattern:** Slice 02's plan registered `PlacementsBackfillService` as an `IHostedService`/`BackgroundService` and treated that as "runs once at Worker startup". In production the Worker is invoked with `BATCH=true`, which causes `Program.cs` to call `processor.UpdateReplay()` and `return` **before** `app.RunAsync()` — so no hosted services ever start, and the backfill silently never ran. The fix was to extract the logic into a directly-callable `PlacementsBackfiller` and invoke it from the batch path alongside `Processor.UpdateReplay()`. The same trap will catch any future "run once at startup" work specced as a `BackgroundService`.
- **Evidence:** PR #1325 (entire PR was a follow-up fix; no human review comments). `src/Worms.Hub.Gateway/Program.cs:85-92` shows the batch path returning before `RunAsync`. Original slice plan `.claude/specs/elo-rankings/slices/02-placement-persistence/plan.md:36,316,344,373` describes the backfill purely as a `BackgroundService` registered via `AddHostedService`. `review.md` accepted the plan as-is and did not catch the gap.
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — expand the `WORMS_BATCH` row in the environment-variables table into an explicit note that batch mode returns before `app.RunAsync()`, so **no `IHostedService` / `BackgroundService` registrations execute in batch mode**. Any startup-only work (backfills, one-shot migrations) must be implemented as a directly-callable class (pattern: `StartupBackfiller` / `PlacementsBackfiller`) and invoked from both the hosted-service wrapper (for normal worker mode) and the `runAsBatchJob` block in `Program.cs` (for production cron runs). Plans that introduce a new backfill must list the `Program.cs` batch-path edit as a required file, and reviewers must check both call sites are wired before approving.

## From PR #1340

### P1340.1 — New repository queries land on the nearest existing repository instead of the one whose aggregate they return

- **Pattern:** The slice added `GetAffectedLeagueIds(machine, teamName)` to `IReplaysRepository` because the query reads from the replays table. Post-merge it was renamed to `GetLeaguesWithTeam` and then moved onto a newly-introduced `ILeaguesRepository`, because the method returns *leagues*, not replays — its aggregate root is the league. Neither the plan nor the agent review questioned the repository placement or the method name; both had to be corrected in follow-up commits after the review.md was written.
- **Evidence:** PR #1340 follow-up commits `b5f5accc` (rename `GetAffectedLeagueIds` to `GetLeaguesWithTeam`) and `c45babb3` (introduce `ILeaguesRepository`, move method off `IReplaysRepository`), both landing after the review.md commit `971011ca`.
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — add guidance that a new repository method belongs on the repository for the aggregate it *returns*, not the table it reads from; if no such repository exists, create one rather than parking the method on an unrelated repository. The plan template should require new queries to name their returned aggregate explicitly, and the agent review must challenge any query whose return type does not match its host repository.

## From PR #1345

### P1345.1 — Review accepted bespoke per-field repo methods instead of reusing the existing aggregate write path

- **Pattern:** Plan and review both accepted new `IReplaysRepository` methods (`UpdatePlacementElo`, `ClearPlacementEloForLeague`) and the resulting non-transactional clear-then-fill window (review S2/S3 both declined as "not worth the churn" and "spec accepts the partial-read window"). Post-review, the author refactored to drop both methods entirely and persist `EloDelta`/`EloAfter` through the existing `Update(replay)` write — collapsing N round-trips per recompute into a single transactional call and removing the partial-read window the review explicitly tolerated. The review reasoned about the partial-read tradeoff without first asking whether the existing aggregate write could absorb the new fields.
- **Evidence:** PR #1345 follow-up commit `b8a54b5f` ("refactor(storage,gateway): write placement ELO via Update(replay) instead of bespoke repo methods"); slices/09-elo-delta-on-game-detail/review.md S2 and S3 both declined the simpler shape.
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — add a rule that when a slice adds new fields to a domain entity that already has an aggregate `Update(entity)` write, the default is to extend that write rather than introduce field-targeted repo methods; both planners and reviewers must flag bespoke per-field methods on the same aggregate as a smell unless there is a stated reason the aggregate write is unsuitable.

### P1345.2 — Default-null parameters on a positional record went unflagged by review

- **Pattern:** New positional record parameters `EloDelta` and `EloAfter` on `ReplayPlacement` were added with `= null` defaults, even though the existing nullable parameter (`PlayerName`) is passed `null` explicitly at every construction site. The defaults make it silently easy for future callers to forget the ELO fields exist. The slice review.md verified the record extension itself but did not check the convention for defaulting nullable record parameters; the issue was caught only when the author re-read the record post-review.
- **Evidence:** PR #1345 follow-up commit `5707f19e` ("refactor(storage): drop default nulls from ReplayPlacement record"); not flagged in slices/09-elo-delta-on-game-detail/review.md.
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add a convention that positional record parameters do not take default values; nullable parameters are passed explicitly at every construction site, so that adding a new optional field forces every existing caller to make a deliberate choice instead of inheriting an implicit `null`.

## From PR #1322

### P1322.1 — Redundant feature-flag gating at the DTO layer when the domain already encodes flag state

- **Pattern:** The plan and agent review both treated "thread `IsPlacementsEnabledAsync()` from controller into `ReplayDetailDto.FromDomain`" as legitimate work, and the review marked the gated DTO projection as MET. In fact, when the flag is off, the older repository never populates `replay.Placements`, so the domain object already carried the right state — the DTO/controller plumbing was pure duplication. Two post-review commits stripped the parameter, and then simplified the empty-to-null coercion (which was itself conflating "data unavailable" with "available but empty").
- **Evidence:** PR #1322 follow-up commits `7438e45` ("remove redundant feature flag from ReplayDetailDto.FromDomain") and `d41aad0` ("simplify placements projection in ReplayDetailDto")
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add a "don't re-gate at the DTO layer" rule: when a feature flag has already shaped the domain object (null vs populated collection), the DTO projection should mirror the domain's nullability rather than re-checking the flag. Reviewers should challenge any flag check that appears below the layer that first read the flag.

### P1322.2 — Parallel data fields retained when one is derivable from the other

- **Pattern:** `ReplayResource` exposed both `Teams` and `Placements`, but every `Placement` carries its `Team`, so `Teams` was always derivable from `Placements`. The plan added `Placements` alongside `Teams` and the agent review confirmed the file list matched the plan, but neither questioned whether the two collections were redundant. A post-review refactor removed `Teams` entirely and updated all read sites (CLI Replays table, Worker `Processor`, `ReplayResourceBuilder`, CLI `LocalReplayRetriever`).
- **Evidence:** PR #1322 follow-up commit `d2ad08d` ("remove Teams from ReplayResource, use Placements as the single source")
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add a "single source of truth for collections" check to the review prompt: when a new collection is added to a resource/DTO that overlaps an existing one, the reviewer must verify each consumer still needs both, or recommend collapsing.

### P1322.3 — Null-position semantics not modelled for retired or unfinished games

- **Pattern:** `PlacementCalculator` returned an empty list when there was no winner or no kills, conflating "no positions resolvable" (retired/unfinished game) with "data unavailable" (older schema). The plan and the agent review both assumed every team always had a non-null position, so `Placement.Position` was non-nullable in the domain, `replay_placements.position` was `NOT NULL`, the CLI printer assumed an integer, the web `PlacementDto.position` was typed `number`, and the UI's sort/medal-lookup dereferenced it unconditionally. Cleaning this up required a domain change, a new migration (V0.6), a feature-flag schema bump, CLI `-` rendering, and a TS type/sort/medal-lookup guard.
- **Evidence:** PR #1322 follow-up commits `0ed88e9` ("show teams with null positions for retired/unfinished games"), `85e0b52` ("bump placements schema check to V0.6"), `df18f31` ("allow null position on PlacementDto in web UI"), `b8d8ceb` (TS `position: number | null` fix and null guards)
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add planning guidance that any new domain field representing a computed rank/position/score must explicitly enumerate the "cannot be computed" case (retired game, no data, tie unbreakable) at the spec stage; nullability decisions must be made once at the domain layer and propagated through migration column constraints, DTO types, CLI rendering, and TS types in the same slice.

### P1322.4 — Seed data not refreshed when new UI surfaces are added

- **Pattern:** The slice added placement chips, finishing-order columns, and a new "Ranking" view, but the local-dev seed data did not include replays with the full log shape (weapons, length, most-damage) needed to populate the new columns, did not cover 2-/3-/4-team variations or a draw or a pending replay, and re-seeding the league data failed with an FK violation because `R__` repeatable scripts run alphabetically (leagues before replays) and the league script used `DELETE` rather than `TRUNCATE ... CASCADE`. The agent review's verdict said "satisfies all acceptance criteria" while three manual test-plan rows in the PR body were left unchecked at merge time, signalling these surfaces were never exercised end-to-end.
- **Evidence:** PR #1322 follow-up commits `7275f45` ("use TRUNCATE CASCADE to clear leagues seed data") and `b8d8ceb` ("expand replay seed data with full logs and varied scenarios"); PR body test plan with three unchecked manual items at merge time
- **Where to fix it:** `.claude/docs/steering/testing-strategy.md` — make seed-data refresh an explicit acceptance criterion for any slice that adds a new column, chip, or page section: the slice must include the seed-data update needed to exercise the new surface in `docker compose up`, and the agent review must treat unchecked manual-test items in the PR body as a blocker rather than reporting "satisfies all acceptance criteria".

## From PR #1324

### P1324.1 — Auth provider name leaked into domain types and DB schema

- **Pattern:** The implementation named the player-identity field `Auth0Subject` on domain records (`Player`, `Team`), DTOs, repository methods, and the `players.auth0_subject` column in the V0.7 migration. The agent's review.md treated this as fine. The user then had to follow up renaming everything to `AuthSubject` so the codebase doesn't bake the current auth provider name into long-lived shapes.
- **Evidence:** PR #1324 follow-up commit `dfa68ce` ("refactor: remove auth0-specific naming from subject fields") touching `TeamsController`, `TeamDtos`, `IPlayersRepository`, `PlayersRepository`, `TeamsRepository`, `Player`, `Team`, and `V0.7__AddPlayersAndTeams.sql`.
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add a "no vendor names in domain or schema" rule: identifiers exposed through domain records, DTOs, repository APIs, and SQL column names must describe the concept (`AuthSubject`, `subject`) rather than the current provider (`Auth0Subject`, `auth0_subject`). Plans and reviews must check for this before merge.

### P1324.2 — Surrogate integer key added when a natural unique key already existed

- **Pattern:** The V0.7 migration created `players` with a surrogate `id SERIAL PRIMARY KEY` alongside a unique `auth_subject` column, and `teams.player_id` referenced the surrogate. Since `auth_subject` is already globally unique and is the only key the application looks rows up by, the surrogate adds an extra join column without value. The agent's review.md did not flag the duplicate identity. The user then dropped the surrogate and made `auth_subject` the primary key, renaming `teams.player_id` to `teams.player_auth_subject`.
- **Evidence:** PR #1324 follow-up commit `54445f3` ("refactor: use auth_subject as natural key for players table") modifying `V0.7__AddPlayersAndTeams.sql`, `Player`, `ITeamsRepository`, `PlayersRepository`, `TeamsRepository`, and `TeamsController`.
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — add a "natural keys over surrogates" guideline for new tables: if a column is already unique, not-null, and the only key the application looks rows up by, make it the primary key rather than adding a `SERIAL` surrogate. Reviewers should challenge any new table whose surrogate is never read by application code.

### P1324.3 — New REST endpoints diverged from the existing PUT-with-id-in-body convention

- **Pattern:** The plan and implementation defined `PUT /teams/{id}` with the id in the route segment. The existing Games endpoints use `PUT /games` with the id in the request body, so the new endpoint was inconsistent with the established gateway convention. The agent's review.md compared the endpoint against the spec but never compared its shape against neighbouring controllers, so the divergence wasn't flagged. The user followed up moving the id into `ClaimTeamDto`.
- **Evidence:** PR #1324 follow-up commit `6103414` ("refactor: move team id from route to body on PUT /teams") modifying `TeamsController`, `TeamDtos`, and `TeamsPage.tsx`.
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — document the REST shape conventions used by the gateway (PUT takes the full resource in the body, including its id; the URL is the collection, not a per-item segment) and add a planning/review step requiring new endpoints to be diffed against the closest existing controller before the slice is approved.

### P1324.4 — Repeated claim-check expression not extracted to a domain method

- **Pattern:** The TeamsController had three callsites computing variants of `team.ClaimedByAuthSubject != null && team.ClaimedByAuthSubject == subject` and `team.ClaimedByAuthSubject != null && team.ClaimedByAuthSubject != subject`. The agent's review.md did not flag this duplication (S1/S2 focused on unrelated concerns). The user followed up adding `IsClaimedBy(subject)` and `IsClaimedByAnother(subject)` helpers on the `Team` record.
- **Evidence:** PR #1324 follow-up commits `e7ac45c` ("refactor: extract IsClaimedBy helper onto Team domain record") and `34bda12` ("refactor: add IsClaimedByAnother helper to Team domain record").
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — strengthen the existing DRY guidance with a specific rule for domain records: when two or more callsites perform the same null-guarded comparison on the same field, the predicate belongs as a method on the record. Reviewers should grep across touched files for repeated property-access patterns before approving.
