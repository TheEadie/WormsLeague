# Retrospective — Worms Hub Web UI

## From slice files

### L1 — CI build jobs conditionally gated on change detection

- **Pattern:** The implementation agent wired `build-web` with an `if: ${{ needs.changes.outputs.web-build == 'true' }}` condition, causing the build to be skipped entirely when non-web files changed. The correct pattern in this repo is that build jobs always run; change detection gates only release/deploy jobs.
- **Evidence:** `.claude/specs/web-ui/slices/01-spa-scaffolding/learnings.md`, `01-spa-scaffolding/review.md` (B1)
- **Where to fix it:** `.claude/docs/steering/ci-patterns.md` already documents this correctly; it should also be reinforced in `.claude/docs/components/web.md` with an explicit note that the `web-build` change-detection output is reserved for a future deploy job and must not be placed on the `build-web` job itself.

### L2 — Lint step ordered before `npm ci` in CI, causing failure on fresh runner

- **Pattern:** The CI workflow placed the `make web.lint` step before `make web.build` in `zz-build-web.yml`. Because `make web.build` runs `npm ci` (installing `node_modules`), the linting tools — ESLint, `tsc`, Prettier — do not exist on a fresh GitHub Actions runner until after the build step. The lint step fails immediately on a clean runner.
- **Evidence:** `.claude/specs/web-ui/slices/01-spa-scaffolding/review.md` (B1)
- **Where to fix it:** `.claude/docs/steering/ci-patterns.md` already documents the required ordering; `.claude/docs/components/web.md` should also explicitly list the mandatory step order for any CI job touching web linting (checkout → setup Node → web.build → web.lint).

### L3 — Repository visibility set to `internal sealed` when cross-assembly injection requires `public`

- **Pattern:** Plans instructed making new `XxxRepository` classes `internal sealed` by analogy with `GamesRepository` and `ReplaysRepository`. Those existing repos are consumed via the `public IRepository<T>` interface, so the concrete type can be `internal`. New repositories (`LeaguesRepository` in slice 10, `ReplaysRepository` made public in slice 11) are injected directly by their concrete type from the Gateway assembly, which requires `public` visibility. This was caught mid-implementation in both slices.
- **Evidence:** `.claude/specs/web-ui/slices/10-league-list/learnings.md`, `.claude/specs/web-ui/slices/11-per-league-page/learnings.md`
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — the "Adding a new domain object" guidance should note that if a repository will be injected directly by concrete type from another assembly (e.g., the Gateway), it must be declared `public sealed` rather than `internal sealed`; use `internal sealed` only when the repository is consumed exclusively via `IRepository<T>`.

### L4 — DB record type named against convention (`LeagueRecord` instead of `LeagueDb`)

- **Pattern:** The implementation named the Dapper mapping type `LeagueRecord` instead of `LeagueDb`, deviating from the `XxxDb` naming convention used by `GamesDb` and `ReplayDb` in the same file and documented in the hub-storage component doc. The plan failed to call out the specific convention name, leading to the deviation being caught only in review.
- **Evidence:** `.claude/specs/web-ui/slices/10-league-list/review.md` (S2)
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — the "Adding a new domain object" step 2 should state the Dapper mapping type must be named `XxxDb` (matching the pattern `GamesDb`, `ReplayDb`) and provide an explicit example to prevent a plausible but wrong name like `XxxRecord`.

### L5 — `AddWormsArmageddonFilesServices()` placed only in the gateway branch, breaking distributed worker-only mode

- **Pattern:** The plan described moving `AddWormsArmageddonFilesServices()` to be unconditional in `Program.cs` (outside both `if (runGateway)` and `if (runWorker)` blocks) because `Processor` in the worker depends on `IReplayTextReader`. The implementation placed the call only inside `if (runGateway)`, which means a distributed worker-only deployment (`HUB_DISTRIBUTED=true`, `HUB_GATEWAY=false`, `HUB_WORKER=true`) cannot start — DI resolution of `Processor` fails at runtime.
- **Evidence:** `.claude/specs/web-ui/slices/12-game-detail-page/review.md` (B1), `12-game-detail-page/learnings.md`
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — the "Service registration" section should document that services shared by both the gateway and the worker (such as `IReplayTextReader` / `AddWormsArmageddonFilesServices`) must be registered unconditionally in `Program.cs`, outside any `if (runGateway)` / `if (runWorker)` block, and should name the shared services explicitly.

### L6 — MUI v9 API differences not reflected in plans (direct prop vs `sx`, `primaryTypographyProps` removed)

- **Pattern:** Plan code examples passed shorthand style props (`fontWeight`, `display`) as direct JSX props on `<Typography>`, and used `primaryTypographyProps` on `<ListItemText>`. Both were valid in earlier MUI versions but are not accepted by MUI v9's TypeScript types. The implementer had to convert these to `sx={{ ... }}` and `slotProps` respectively mid-flight. The plan was authored against MUI v9 but the examples were drawn from older patterns.
- **Evidence:** `.claude/specs/web-ui/slices/12-game-detail-page/learnings.md`; also noted as a pre-existing doc staleness in `.claude/specs/web-ui/slices/06-dark-mode/review.md` (N1)
- **Where to fix it:** `.claude/docs/components/web.md` — add a "MUI v9 API notes" section that documents: (1) scalar style shorthands (`fontWeight`, `display`, etc.) must go in the `sx` prop, not as direct component props; (2) `primaryTypographyProps` / `secondaryTypographyProps` on `<ListItemText>` are removed — use `slotProps={{ primary: { ... } }}` instead.

### L7 — `make web.build` called in CI jobs that only need `npm ci`

- **Pattern:** Plans initially specified running `make web.build` (which runs `npm ci` then the full Vite bundle compilation) in every lint/scan job, including CodeQL and the ESLint SARIF job, even though those jobs do not need a built bundle. This was corrected mid-flight in slice 02: CodeQL needs neither Node nor `node_modules`; ESLint and Prettier need `npm ci` but not Vite build. Calling `make web.build` in a scan job wastes CI time by compiling a bundle that is discarded immediately.
- **Evidence:** `.claude/specs/web-ui/slices/02-web-linting-in-ci/learnings.md`
- **Where to fix it:** `.claude/docs/steering/ci-patterns.md` — add guidance that code-scanning jobs must call only `npm ci` (inline) rather than `make web.build` (which also compiles the bundle), except for jobs that actually require the built artefact; note that CodeQL with `build-mode: none` needs neither step.

### L8 — Dockerfile base images added with floating tags; repo convention requires digest-pinned images

- **Pattern:** The Dockerfile created in slice 03 used floating tags (`node:22-alpine`, `nginx:alpine`) while every existing Dockerfile in the repo pins base images with `@sha256:` digests. The deviation was caught only in review and flagged as a suggestion rather than a blocker, but it is a recurring implicit convention that the implementation agent missed.
- **Evidence:** `.claude/specs/web-ui/slices/03-local-dev-integration/review.md` (S1)
- **Where to fix it:** `.claude/docs/components/web.md` — add a note under a "Docker" section stating that all base images in `build/web/Dockerfile` must be pinned with `@sha256:` digests, consistent with the convention in `build/docker/gateway/Dockerfile` and `build/docker/wa-runner/Dockerfile`.

### L9 — `[PublicAPI]` annotation omitted from new repository classes

- **Pattern:** `GamesRepository` and `ReplaysRepository` carry `[PublicAPI]` on their DB record types to document that Dapper and DI resolve them reflectively. When a new repository (`LeaguesRepository`) was added, the `[PublicAPI]` annotation was omitted and had to be added after review feedback.
- **Evidence:** `.claude/specs/web-ui/slices/10-league-list/review.md` (S1)
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — the "Adding a new domain object" step 2 should include `[PublicAPI]` in the checklist for the `XxxDb` record type, noting that it marks the type as resolved by Dapper/DI and suppresses false-positive unused-code warnings.

### L10 — Unplanned prerequisite migration (`V0.3.1`) added without a learnings entry

- **Pattern:** Slice 11 required a new versioned migration (`V0.3.1__SeedRedgateLeague.sql`) as a logical prerequisite for the FK backfill in `V0.4.1`. The migration was added correctly but was not listed in the plan and not mentioned in `learnings.md`, leaving a gap in the audit trail that was flagged in review.
- **Evidence:** `.claude/specs/web-ui/slices/11-per-league-page/review.md` (S1), `11-per-league-page/learnings.md`
- **Where to fix it:** The `implement-slice` skill's process guidance (or `.claude/docs/steering/` if a general implementation-process doc exists) should require that any file added outside the plan is recorded in `learnings.md` — the existing guidance covers this for new files but the agent overlooked it here specifically for migration files.

### L11 — `UseRequestLogging()` placed after endpoint mapping, silently missing static-file and fallback requests

- **Pattern:** In slice 13, `UseRequestLogging()` was added after the `Map*` endpoint registration calls in `Program.cs`. ASP.NET Core middleware placed after endpoint registrations runs after endpoint dispatch, so requests short-circuited by `UseStaticFiles()` or `MapFallbackToFile()` are never seen by the logging middleware.
- **Evidence:** `.claude/specs/web-ui/slices/13-production-deployment/review.md` (S1)
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — add a note to the "Service registration" or "Configuration" section documenting the required middleware ordering: `UseRequestLogging()` (and any other cross-cutting middleware) must be registered before `UseStaticFiles()`, `MapControllers()`, and `MapFallbackToFile()` to ensure all request paths are covered.

## From PR #1307

### P1307.1 — Agent's recommended fix for shared-service DI placement contradicted the repo's self-contained-registration pattern

- **Pattern:** The review.md for slice 12 identified the correct blocker (B1: `AddWormsArmageddonFilesServices()` placed inside the gateway-only block) and recommended moving it "outside the `if (runGateway)` block" — i.e., unconditionally in `Program.cs`. The human reviewer rejected that approach: the correct fix is to call it inside both `AddGatewayServices()` and `AddWorkerServices()`, keeping each mode's registration self-contained. However, that approach required a prerequisite the agent's review did not identify: `AddWormsArmageddonFilesServices()` used `AddScoped`/`Add` for its registrations, making repeated calls in monolith mode unsafe (double-registered `IReplayLineParser` parsers, every parser running twice per line). A separate refactor commit was needed first to switch to `TryAddScoped`/`TryAddEnumerable` so the method became idempotent. The agent's fix recommendation was technically coherent but missed both the repo's preferred pattern and the prerequisite idempotency work.
- **Evidence:** PR #1307 inline review comment by TheEadie on `Program.cs:55`; follow-up commits `ef8f2345` (make `AddWormsArmageddonFilesServices` idempotent) and `444e6f52` (move call into component methods)
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — the "Service registration" guidance introduced for L5 should be updated: replace the instruction to call shared services unconditionally in `Program.cs` with the correct pattern: (1) call the shared service registration method inside each component's `Add*Services()` method; (2) any such method that will be called from multiple component methods must use `TryAddScoped`/`TryAddSingleton`/`TryAddEnumerable` throughout so repeated calls in monolith mode are safe.

### P1307.2 — Plan did not flag that a richer DTO on the single-item endpoint makes the list endpoint inconsistent

- **Pattern:** Slice 12 introduced `ReplayDetailDto` (with parsed turns data) for `GET /leagues/{id}/replays/{replayId}` while leaving `GET /leagues/{id}/replays` returning the lighter `ReplayDto`. The spec and plan did not note this asymmetry as a scope decision, so the review.md passed the PR without flagging it. The reviewer caught it post-review: the league replay list should use the same richer shape, filling the previously-empty Top Weapons, Length, and Most Damage columns. This triggered four substantial unplanned commits that touched both the gateway controller and the SPA.
- **Evidence:** PR #1307 issue comment by TheEadie ("The league replay list should be updated with the details we now have available following this change"); follow-up commits `a242dc94`, `02486247`, `2d3e39b3`, `48e42cf8`
- **Where to fix it:** The `spec` and `plan-spec` skill guidance should add a prompt: when a new endpoint introduces a richer DTO for a single-item response, explicitly decide whether the corresponding list endpoint should return the same shape — and record that decision (in scope or out of scope) in the spec. Leaving the decision implicit causes the asymmetry to be caught only in human review and treated as unscoped follow-up work.

### P1307.3 — Prettier not run after post-review SPA changes, requiring a separate clean-up commit

- **Pattern:** After four post-review SPA feature commits, a standalone `chore(web): fix Prettier formatting` commit was needed before the PR could merge. The agent's review-response workflow for SPA changes did not include running `make web.lint` as a final step before committing follow-up work.
- **Evidence:** PR #1307 commit `4082182b` "chore(web): fix Prettier formatting", pushed after the four post-review feature commits
- **Where to fix it:** `.claude/docs/components/web.md` — reinforce in the web development workflow section (alongside the existing Prettier guidance) that `make web.lint` must be run and pass before committing any SPA changes, including follow-up commits made in response to review feedback. Prettier diffs are a CI failure and must be caught locally before push.

## From PR #1305

### P1305.1 — Independent gateway/DB deployment risk not assessed during slice spec

- **Pattern:** Slices 10 and 11 added new endpoints backed by a DB schema migration (V0.3), but neither the spec nor the plan considered what would happen if the gateway was deployed before the migration ran. In this deployment model the gateway and the database are updated independently, so deploying the new code against the old schema crashed the gateway rather than degrading gracefully. The gap was only discovered when the feature hit production, requiring a separate unplanned PR to add schema-version gating.
- **Evidence:** PR #1305 existence — "feat: degrade gracefully when DB schema is behind gateway version", merged 2026-05-13, with no corresponding slice in the web-ui epic plan
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — add a "Deployment safety" section stating that any new endpoint backed by a DB migration must either (a) gate on schema version before querying the new table, or (b) be hidden behind a feature flag until the migration has been confirmed applied; the spec template (or `.claude/docs/steering/`) should include a "deployment risk" checklist item asking whether independent gateway/DB rollout can cause a crash.

### P1305.2 — Missing braces on `if` statements generated by implementation agent, caught by InspectCode in CI

- **Pattern:** The initial implementation commits omitted braces on single-statement `if` blocks across `LeaguesController.cs` and `DatabaseSchemaVersion.cs`. Three InspectCode violations were flagged by the GitHub Advanced Security bot in review, requiring a follow-up style-fix commit (`7b97822`). The project enforces "always use braces" via InspectCode, and the rule fires as a build warning catchable by `dotnet build --warnaserror` before pushing.
- **Evidence:** PR #1305 review comments by `github-advanced-security[bot]` (comment IDs 3232646781, 3232646807, 3232646815); follow-up commit `7b97822`
- **Where to fix it:** `.claude/docs/steering/coding-guidelines.md` — add an explicit rule that `if`, `else`, `for`, `foreach`, and `while` blocks must always use braces, even for single-statement bodies, and note that `dotnet build --warnaserror` will surface InspectCode violations before pushing.

### P1305.3 — Feature-flag abstraction omitted in initial implementation, requiring immediate refactor

- **Pattern:** The first version of `LeaguesController` injected `DatabaseSchemaVersion` directly as a constructor parameter. A follow-up commit (`00612c90`) on the same PR immediately extracted `IFeatureFlags` / `GatewayFeatureFlags` to centralise feature decisions and allow multiple sources (env-var overrides, schema version, external services) to be aggregated without touching controllers. The plan produced working code but missed the abstraction layer, making the refactor a necessary second commit rather than being correct first time.
- **Evidence:** PR #1305 commit `00612c90` — "refactor: extract IFeatureFlags so feature gating can aggregate multiple sources"
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — document the `IFeatureFlags` / `GatewayFeatureFlags` pattern: controllers must depend on `IFeatureFlags`, not on `DatabaseSchemaVersion` or any other concrete source directly; any new schema-version or feature gate check must be added to `GatewayFeatureFlags` rather than inlined into a controller.

## From PR #1289

### P1289.1 — Plan asserted existing code state without verifying it

- **Pattern:** The plan for slice 04 stated "The current code in `Program.cs` does not call `UseRouting()` explicitly" and included an instruction to add an explicit `UseRouting()` call. In fact `UseRouting()` was already present. The plan-writing agent read `Program.cs` as required by the `plan-spec` process but still produced a false description of what it contained. The implementer had to silently skip the planned step. In a less forgiving context — for example, if a duplicate middleware registration caused a runtime error — this could have introduced a regression.
- **Evidence:** PR #1289 `learnings.md` ("UseRouting() was already present in Program.cs") and `plan.md` section 5 ("The current code in `Program.cs` does not call `UseRouting()` explicitly")
- **Where to fix it:** `.claude/commands/plan-spec.md` Step 2 — add a reminder that when the plan makes a factual claim about the current state of a file (e.g. "X is not present", "the file currently has only Y sections"), that claim must be verified by reading the actual file content, not asserted from assumption or general knowledge. Plans that mis-describe absent things as present (or vice versa) force silent deviations during implementation and can mask real bugs.

## From PR #1281

### P1281.1 — Review agent read plan text rather than actual diff, producing false findings

- **Pattern:** Across multiple slices, the review agent checked what the plan said should be done rather than reading the actual files on disk. This manifested in four distinct failure modes: (1) verifying acceptance criteria as MET based on plan wording when the implementation had deliberately deviated (PR #1281 — `make web.build` vs inline `npm ci`); (2) declaring "no unexplained deviations" while missing files present in the diff but absent from the plan (PR #1298 — `api.ts`, `.env`); (3) raising suggestions flagging things as missing or wrong that were already correct in the committed code (PR #1301 — `[PublicAPI]` and `XxxDb` naming already present; PR #1291 — `width: '100%'` already present, `gap: 3` never added); (4) reporting a bug the implementation had already self-corrected, citing a line number from the plan's code example rather than the actual file (PR #1309 — `UseRequestLogging()` placement already fixed to line 63 before review was written). In all cases the review agent was referencing its mental model or the plan's text rather than the committed files.
- **Evidence:** PR #1281 review.md (AC row 6 false pass vs actual diff in `code-scanning.yml`); PR #1298 review.md ("No unexplained deviations" and S2 for work already done in `api.ts`); PR #1301 review.md (S1 and S2 false alarms vs `git show d81aa07d`); PR #1291 commit `e6ec08b9` vs review.md S1 and N1; PR #1309 review.md S1 citing `Program.cs:81` vs implementation commit `d37fa00f` which had moved `UseRequestLogging()` to line 63
- **Where to fix it:** The `review` skill's process guidance — every claim about what was or was not implemented must be verified by reading the current file at the specific line before raising it. Acceptance criteria must be verified against the diff, not plan wording. The scope check must enumerate files actually in the diff and compare against the plan's table. Suggestions stating "X is missing" must quote the actual line from the file. If the implementation diverges from the plan beneficially, the review must record the divergence — not report the correct implementation as a bug.

## From PR #1300

### P1300.1 — Security-critical UI components planned and implemented without automated tests

- **Pattern:** The spec and plan for `RequireAuth` — described in the spec itself as "the single place where this redirect logic lives" for all protected routes — listed only `make web.build` and `make web.lint` in its acceptance criteria. No test requirement was stated. The plan explicitly declared "No new dependencies are needed." The review's S1 caught the gap and triggered three unplanned commits: installing Vitest + React Testing Library (package.json, vite.config.ts, tsconfig.app.json, test-setup.ts), writing three branch-coverage tests, and adding a `make web.test` target plus a parallel CI test job. A component that is the sole auth enforcement point across the entire SPA warrants automated test coverage as a first-class requirement, not an optional post-review addition.
- **Evidence:** PR #1300 review.md S1 decision ("Accept — Vitest + React Testing Library installed; three tests covering loading/unauthenticated/authenticated branches written"); follow-up commits `3e576c7` (install Vitest/RTL), `9ab3cd0` (tests), `769795e` (make target + CI jobs)
- **Where to fix it:** `.claude/docs/components/web.md` — add a "Testing" section documenting: (1) Vitest + React Testing Library is the web test framework; (2) any component that is the single enforcement point for a security or routing invariant (e.g. an auth guard) must have automated tests covering all branches as part of its slice spec, not deferred; (3) the `make web.test` target and CI job structure (explicit `npm ci` then `make web.test`, separate from the package job that runs `make web.build`). Also update `.claude/docs/steering/testing-strategy.md` to include the web tier alongside the .NET tiers.

### P1300.2 — Spec acceptance criteria did not include `make web.test`; test infrastructure absent from plan scope

- **Pattern:** When a slice introduces test infrastructure for the first time (Vitest, a new CI job, a new make target), the spec's acceptance criteria and the plan's scope must explicitly cover that infrastructure — otherwise the plan's "files to create/modify" table, verification steps, and dependency list are all incomplete. The plan stated the only changes were `RequireAuth.tsx` and `App.tsx`; the actual PR added six additional files and a new CI job. The mismatch meant the review had to expand scope rather than confirm it.
- **Evidence:** PR #1300 plan.md ("Files to Create / Modify" lists only two files; "No new dependencies are needed"); follow-up commits `3e576c7`, `769795e` adding five files and CI workflow changes that were never in the plan
- **Where to fix it:** The `spec` and `plan-spec` skills' process guidance should note that if a slice is the first to exercise a capability (test framework, CI job type, make target category), the spec must include setup of that capability in scope and the plan must list every file that will be created or modified, including configuration files (`vite.config.ts`, `tsconfig.app.json`, CI workflow files) and lock files.

## From PR #1309

### P1309.2 — Plan-spec agent copied the existing wrong middleware order into the plan's code example

- **Pattern:** The plan for slice 13 included a complete `Program.cs` code block with `UseRequestLogging()` placed last, after `MapFallbackToFile()` — reproducing the already-wrong ordering that existed in the pre-existing file. The plan-spec agent read `Program.cs` (as required) but transcribed the existing wrong order into the plan example rather than applying the correct ASP.NET Core pattern (logging middleware must precede endpoint dispatch). The implementer caught the issue independently and corrected it during the same commit, so no bug shipped — but a plan that prescribes a known-wrong pattern increases the risk that a future implementer follows it verbatim.
- **Evidence:** PR #1309 `plan.md` section 3 Program.cs code block (last two lines: `_ = app.MapFallbackToFile("index.html"); _ = app.UseRequestLogging();`); implementation commit `d37fa00f` which moves `UseRequestLogging()` to line 63, the first statement inside `if (runGateway)`.
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — apply the fix already flagged by L11: add an explicit note that `UseRequestLogging()` must be the first middleware call inside `if (runGateway)`, before `UseStaticFiles()`, `MapControllers()`, and `MapFallbackToFile()`. Once that guidance is in place, the `plan-spec` skill should read `hub-gateway.md` before writing any `Program.cs` code examples so the plan's code reflects the documented convention rather than whatever the current file happens to contain.

## From PR #1285

### P1285.1 — Implementation agents pinned outdated npm dependency versions

- **Pattern:** The implementation agents that built the web UI scaffolding and subsequent slices selected npm package versions from their training data rather than resolving the current latest at implementation time. This caused a full catch-up PR at the end of the epic bumping every major dependency (React 18→19, MUI 6→9, Vite 5→8, TypeScript 5→6, ESLint 9→10). The problem compounds because later slices (e.g. L6's MUI v9 API notes) had to work around API differences between the version used in plan examples and the actually-installed version.
- **Evidence:** PR #1285 — existence of the PR itself; the commit message explicitly lists five major-version upgrades that had accumulated across the epic implementation. No human reviewer flagged this during any slice review, confirming it was silent drift rather than a conscious pinning decision.
- **Where to fix it:** `.claude/docs/components/web.md` — add a note in a dependency management section stating that when adding or updating npm packages, agents must resolve the current latest version at implementation time (e.g. via `npm show <pkg> version`) rather than using versions from training data; versions in `package.json` should always reflect the latest available at the time of writing, not stale values from model knowledge.

## From PR #1296

### P1296.1 — `tsc --noEmit` against the root tsconfig is a silent no-op; `tsc -b` is required

- **Pattern:** `make web.lint` invoked `tsc --noEmit` against the root `tsconfig.json`, which has `files: []` combined with `references`. This configuration makes `tsc --noEmit` check nothing — it exits zero while type-checking zero files. A bad deep-path import in `ColourSchemePicker.tsx` (`@mui/system/cssVars/useCurrentColorScheme`, absent from @mui/system's exports map) went undetected by local linting and only surfaced when Docker ran `tsc -b` inside the container. The fix — changing `make web.lint` to call `tsc -b` — was bundled into PR #1296 rather than caught before implementation began. The review.md wrote that `make web.lint` "passes cleanly" without noting that the typecheck had been inoperative before the fix landed in this same PR.
- **Evidence:** PR #1296, follow-up commit d8956dc3 ("fix: unbreak docker web build and make lint actually typecheck")
- **Where to fix it:** `.claude/docs/components/web.md` — document that `make web.lint` uses `tsc -b` (not `tsc --noEmit`) and explain why: the root `tsconfig.json` has `files: []` + `references`, so `tsc --noEmit` against it is always a no-op. Any plan step or CI guidance that writes `tsc --noEmit` for the web project is incorrect; the correct invocation is `tsc -b`.

### P1296.2 — Hardcoded `calc(100vh - Npx)` height for page content breaks footer visibility

- **Pattern:** The plan specified `minHeight: { md: 'calc(100vh - 52px)' }` on the `LandingPage` grid, intending to fill the viewport below the sticky header. The `Layout` component already establishes a `display: flex; flexDirection: column; height: 100vh` shell, so the correct way for a page's content area to consume the remaining space is `flex: 1` on its outermost element — not viewport arithmetic. Using `minHeight: calc(100vh - 52px)` made the page body fill the full remaining viewport on its own, pushing the footer below the fold. Two follow-up commits were required to converge on the correct fix (`flex: 1` inside a flex-column `main`).
- **Evidence:** PR #1296, follow-up commits 9bde4f86 ("fix: let footer fit on initial viewport on the landing page") and 1a25c06d ("fix: vertically center landing-page content in available space")
- **Where to fix it:** `.claude/docs/components/web.md` — add a layout note stating that page components rendered inside `Layout` must size themselves with `flex: 1` on their outermost element; explicitly prohibit `minHeight: calc(100vh - Npx)` patterns that hardcode component pixel heights, and provide the correct pattern as a short example.

## From PR #1299

### P1299.1 — `Math.random()` inside `useMemo` violates React purity rules and triggers ESLint error

- **Pattern:** The implementation used `Math.random()` inside `useMemo(() => ..., [])` to pick a random weapon name once on mount. The repo's ESLint config includes the `react-compiler/react-compiler` rule, which treats `Math.random()` as an impure function and rejects any call to it during render — including inside `useMemo`. The correct pattern for one-time random initialisation is a lazy `useState` initialiser (`useState(() => pick())`), which runs outside the render path. The agent was unaware of this constraint; the error was caught only by the automated code-scanning bot, not during implementation.
- **Evidence:** PR #1299 code-scanning comments by github-advanced-security[bot] on commit 569875b (code-scanning alerts #368 and #369); issue resolved in follow-up commit 92c89f6
- **Where to fix it:** `.claude/docs/components/web.md` — add a "React purity" note: values that depend on impure functions (e.g. `Math.random()`, `Date.now()`) and should be computed once on mount must be placed in a lazy `useState` initialiser (`useState(() => ...)`) rather than `useMemo(() => ..., [])`, because the `react-compiler/react-compiler` ESLint rule rejects impure calls anywhere in the render path including inside `useMemo`.

### P1299.2 — Landing page spec finalised before the design mockup was approved, requiring a wholesale follow-up PR

- **Pattern:** PR #1299 was a follow-up that applied the agreed final design to the landing page after the mockup was approved — adding typewriter animation, new hero copy, a subtitle, footer removal, and a layout change. None of these were in the original landing-page slice. The original spec was written and implemented before the design was stable, so when the mockup was finalised the entire page had to be redone in a separate PR. The process did not guard against speccing UI work ahead of a settled design.
- **Evidence:** PR #1299 existence and description ("Landing page improvements" applying design changes post-mockup finalisation); the PR body describes structural and copy changes not present in the original implementation
- **Where to fix it:** The `spec` skill guidance should note that UI slice specs must not be finalised until the visual design mockup has been approved; if the mockup is still in flux at spec time, the spec should flag which visual details are pending and explicitly defer those details to a follow-up slice rather than spec'ing a placeholder that will need wholesale replacement.


## From PR #1276

### P1276.1 — Review agent confirmed a convention-violating CI conditional as correct

- **Pattern:** The review.md for slice 01 marked "Branch not touching web paths skips CI web job — MET", treating the conditional `if: ${{ needs.changes.outputs.web-build == 'true' }}` on `build-web` as correct because it matched the spec's acceptance criterion wording. It is actually the wrong pattern: in this repo, build jobs always run and change detection gates only release/deploy steps. The review passed the check based on literal criterion wording without cross-referencing repo conventions, producing a false-positive that required a follow-up fix commit after the review was written.
- **Evidence:** PR #1276, `.claude/specs/web-ui/slices/01-spa-scaffolding/review.md` (acceptance criteria row "Branch not touching web paths skips CI web job — MET"); follow-up commit `b074b216` ("Fix build-web change detection: always build, gate only release")
- **Where to fix it:** The `review` skill's process guidance should require that CI-related acceptance criteria are validated against the repo's CI conventions (documented in `.claude/docs/steering/ci-patterns.md`) and not only against the spec wording. A criterion like "job skips on unrelated changes" must be flagged if it conflicts with the always-build pattern.

### P1276.2 — Prettier configured but not wired into enforcement; review didn't catch the gap

- **Pattern:** `.prettierrc` was committed and `prettier` was listed in `devDependencies`, but `web.lint` did not invoke `prettier --check`. Prettier was installed but silent — formatting drift would go undetected in CI. The review confirmed all acceptance criteria as met and made no mention of this gap, leaving the fix to a separate post-review commit. A tool being present in the project without a corresponding enforcement invocation is a latent process failure.
- **Evidence:** PR #1276 initial commits (`b4a4b8e8`, `dc74b455`); follow-up commit `96fc5a08` ("Add Prettier enforcement to web.lint and format source files")
- **Where to fix it:** `.claude/docs/components/web.md` — the `web.lint` description should explicitly list every tool that must be invoked (`eslint`, `tsc --noEmit`, `prettier --check`) so both the implementation and review agents have a concrete checklist. The `review` skill's guidance should include a check that every configured formatting/linting tool has a corresponding CI invocation.

### P1276.3 — Review confirmed `make test` wired to linting, which violates repo convention

- **Pattern:** The initial implementation added `test:: web.test` to `build/web/makefile`, making `make test` run ESLint and `tsc --noEmit`. This matched the spec's acceptance criterion wording exactly, so the review marked it MET. However, the repo's actual convention is that `make test` runs tests only; linting belongs in `code-scanning.yml` as SARIF uploads, not in the build/test pipeline. The review confirmed a criterion without checking whether the criterion itself was sound, requiring a follow-up fix to remove the `test::` hook and the Lint step from `zz-build-web.yml`.
- **Evidence:** PR #1276, `.claude/specs/web-ui/slices/01-spa-scaffolding/review.md` (acceptance criteria row "`make test` runs ESLint and tsc --noEmit — MET"); follow-up commit `9e2f7836` ("Move web linting out of make test; linting belongs in Code Scanning")
- **Where to fix it:** `.claude/docs/steering/testing-strategy.md` already has guidance on this separation; `.claude/docs/components/web.md` should explicitly state that `web.lint` is a standalone target for local use and code-scanning integration only — it must not be wired to `test::`. The `review` skill should cross-check `make test` additions against the testing-strategy doc rather than trusting spec wording alone.

## From PR #1304

### P1304.1 — Parallel DTO created instead of extending existing type; computed `bool` field diverged from domain model

- **Pattern:** The spec defined `processed: bool` as a computed field in the API response, and the plan followed by creating a dedicated `ReplayInLeagueDto` type. The existing `ReplayDto` already mapped the same domain record and already carried `Status: string`. The new type was a parallel duplicate that the plan should have collapsed into the existing DTO. The `bool Processed` field also diverged from the domain model's `string Status` — the TypeScript interface on the frontend reflected this divergence, causing a runtime mismatch (`replay.processed` did not exist on the actual JSON response) that required a separate fix commit. When an existing DTO covers the same domain object, the plan must extend it rather than create a parallel type, and derived boolean fields must not be introduced when the underlying discriminant is already present in the domain model as a string.
- **Evidence:** PR #1304 follow-up commits `2e3e09b` (refactor: consolidate ReplayInLeagueDto into ReplayDto) and `9eaf6cf` (fix: check status string instead of missing processed boolean in LeagueDetailPage)
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — add guidance that when adding a new endpoint returning the same domain type as an existing endpoint, the plan must reuse and extend the existing DTO rather than creating a parallel type; also note that derived bool fields (e.g. `Processed`) must not be introduced when the domain model already carries an equivalent discriminant (e.g. `Status`), because the frontend TypeScript interface must match the actual serialised shape.

### P1304.2 — No schema-compatibility consideration in plan when adding DB columns the gateway reads

- **Pattern:** The plan added V0.4 columns (`league_id`, `date`, `winner`, `teams`) and updated the repository and endpoint without any consideration of what happens when the gateway runs against a database that has not yet been migrated to schema 0.4 (e.g. during a rolling deployment or when pointing at a shared staging DB). The omission required significant post-review rework: adding a `DatabaseSchemaVersion` service, gating the new endpoint and repository queries behind a version check, splitting `ReplaysRepository` into a base class and a V04 subclass, then further refactoring to remove the inheritance in favour of two independent sealed classes resolved by a DI factory. Any plan that adds columns to an existing table and extends the repository to read those columns must include a compatibility section specifying whether the gateway degrades gracefully on an older schema and, if so, how (feature-flag via schema version, split repository, null-returning fallback, etc.).
- **Evidence:** PR #1304 follow-up commits `a697630` (feat: guard replay league fields behind schema version check), `4aa5329` (refactor: split ReplaysRepository into base and V04 subclass), `60aeafc` (refactor: remove inheritance between replay repositories), `4f9571e` (refactor: make replay repository implementations internal; expose via interfaces), `1931449` (refactor: replace split interfaces with single IReplaysRepository)
- **Where to fix it:** `.claude/docs/components/hub-storage.md` — add a "Schema compatibility" note to the "Adding a new domain object" or migration guidance: when a slice adds columns to an existing table and the gateway reads those columns, the plan must include an explicit compatibility decision (degrade gracefully vs. require DB upgrade before gateway deploy) and, if degrading gracefully, describe the `DatabaseSchemaVersion` DI-factory pattern that selects between a base repository and a versioned subtype.

### P1304.3 — Write endpoints not audited when a new column with a default is added to an existing table

- **Pattern:** When `league_id` was added to the `replays` table and all existing rows were backfilled to `'redgate'`, the plan updated `ReplaysRepository.Create()` and `ReplaysRepository.Update()` to write the new column but did not audit every write path in the Gateway. `ReplaysController.Post` (the replay upload endpoint) also constructs a `new Replay(...)` and calls `repository.Create()` — it was left with `league_id = null`, meaning newly uploaded replays silently had no league association and would not appear on any league page. Whenever a new non-nullable-in-practice column is added to an existing table with a backfill migration, the plan must enumerate all write paths (controllers, workers, tests) that construct or persist the affected record and verify each one sets the new field.
- **Evidence:** PR #1304 follow-up commit `1cd09c5` (fix: default new replay uploads to redgate league)
- **Where to fix it:** `.claude/docs/components/hub-gateway.md` — add a checklist item to guidance on extending an existing domain record: when a new column is added to an existing table, search for all controller actions and worker methods that call `repository.Create()` or `repository.Update()` for that type and confirm each one sets the new field; do not rely on the repository implementation alone to surface missing write-path callers.

## From PR #1291

### P1291.2 — `Dockerfile.dockerignore` `**` catch-all silently excludes new `build/web/` files

- **Pattern:** `build/web/Dockerfile.dockerignore` opens with `**` (exclude everything) and re-allows only `!/src/Worms.Hub.Web`. Any file added to `build/web/` that a Dockerfile `COPY` instruction needs (e.g. `nginx.conf`) is silently excluded from the Docker build context, causing the build to fail with a "not found" error. The workaround — adding `!/build/web/<filename>` — was discovered during implementation and captured in the slice learnings, but the component doc was never updated, so future implementers will hit the same wall.
- **Evidence:** PR #1291, commit dce153fe ("feat: add nginx config for SPA routing and wire into Docker image"), `.claude/specs/web-ui/slices/05-public-landing-page/learnings.md` entry "build/web/Dockerfile.dockerignore uses a `**` catch-all".
- **Where to fix it:** `.claude/docs/components/web.md` — under the "Docker" section (or a new one if it does not exist), add a note that `build/web/Dockerfile.dockerignore` uses a `**` deny-all pattern; any file in `build/web/` that a `COPY` instruction needs must be explicitly whitelisted with `!/build/web/<filename>` in that file.
