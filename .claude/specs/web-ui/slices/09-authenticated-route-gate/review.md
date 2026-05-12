# Review — Authenticated Route Gate

## Verdict

The implementation satisfies every acceptance criterion in the spec. Both `make web.build` and `make web.lint` pass cleanly. The two files changed are exactly what the plan described — no scope drift, no unexplained deviations. There are no blockers. One suggestion is raised about the absence of component-level tests; it is a non-mandatory gap the spec deliberately left open, not a defect.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| A signed-out visitor who navigates to `/authenticated` is redirected to `/` | MET | `RequireAuth.tsx:15-17` — `!auth.isAuthenticated` returns `<Navigate to="/" replace />` |
| A signed-in user who navigates to `/authenticated` sees the page render normally | MET | `RequireAuth.tsx:19` — falls through to `<>{children}</>` when authenticated |
| `/` and `/callback` remain accessible without signing in | MET | `App.tsx:14-15` — both routes are unwrapped, only `authenticated` is wrapped |
| Hard-refreshing on `/authenticated` while signed in keeps the user on that page | MET | `RequireAuth.tsx:11-13` — `auth.isLoading` returns `null` until re-hydration completes, preventing a spurious redirect |
| Hard-refreshing on `/authenticated` while signed out redirects to `/` | MET | Same `isLoading` guard fires first; once loading completes `isAuthenticated` is false, triggering redirect |
| `make web.build` and `make web.lint` both pass | MET | Both commands ran clean in this review run |

## Scope

The diff matches the plan's "Files to Create / Modify" table exactly:

- `src/Worms.Hub.Web/src/components/RequireAuth.tsx` — created as planned
- `src/Worms.Hub.Web/src/App.tsx` — updated with the `RequireAuth` import and wrapper as planned

One additional file changed: `.claude/specs/web-ui/plan.md` — the slice checkbox was ticked from `[ ]` to `[x]`. This is a workflow artefact (process tracking), not feature code, and is ignored per the review rules.

Prettier expanded the inline JSX in `App.tsx` to a multi-line form. This is noted and explained in `learnings.md` — deviation is resolved.

## Blockers

None.

## Suggestions

### S1 — No automated tests for `RequireAuth`

- **File:** `src/Worms.Hub.Web/src/components/RequireAuth.tsx`
- **Issue:** The three routing behaviours — `null` while loading, redirect when signed out, render children when signed in — are currently only verifiable by manual browser testing. No test framework (Vitest, React Testing Library, etc.) is set up in the web project.
- **Fix:** This is not an immediate blocker — the project has no web test infrastructure at all, and adding it is clearly out of scope for this slice. The suggestion is to track adding a test framework as a future slice or tech-debt item so that as `RequireAuth` is the single authoritative guard for all protected routes, it accrues automated regression coverage before more routes are added behind it.
- **Decision:** Accept — Vitest + React Testing Library installed; three tests covering loading/unauthenticated/authenticated branches written in `RequireAuth.test.tsx`; `make web.test` target added.

## Nitpicks

None.

## Tests

No automated test coverage was added. The web project has no test framework installed (`package.json` has no `vitest`, `jest`, or `@testing-library` entries). The spec does not require tests, and the testing-strategy doc covers only .NET tiers for the current test infrastructure. The absence is expected and consistent with the rest of the web slices to date.

The three branches in `RequireAuth` are simple enough that they can be verified manually (as the plan's verification steps describe), and the build/lint pass confirms the TypeScript types are correct.

## Recommended Actions

- **S1** — Resolved — Vitest + RTL test infrastructure added; three tests written for `RequireAuth`.
