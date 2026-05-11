# Review — Browser Sign-In

## Verdict

The implementation fully satisfies the spec. All nine files described in the plan are present and match their intended shapes. `make web.build` and `make web.lint` (ESLint, `tsc --noEmit`, Prettier) all pass cleanly. There are no blockers. A small number of low-severity suggestions and nitpicks are noted below, none of which affect correctness.

---

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Clicking "Sign in" redirects the browser to Auth0 | MET | `LandingPage.tsx:74` — `onClick={() => void auth.signinRedirect()}` |
| After Auth0 authentication the browser lands on the temporary authenticated page (not `/callback` or landing) | MET | `CallbackPage.tsx:13-15` — navigates to `/authenticated` once `auth.isAuthenticated` is true; `onSigninCallback` in `auth.ts:17-19` clears query params |
| The temporary page displays the result of `GET /api/v1/games` without a 401 error | MET | `AuthenticatedPage.tsx:16-24` — fetches with `Authorization: Bearer` header; renders list, empty state, or error |
| When signed in, the header shows the user's username | MET | `Header.tsx:10,40-46` — `auth.user?.profile.nickname ?? name ?? sub` shown in a `Button` when authenticated |
| Clicking username opens a menu with "Sign out"; triggering it clears the session and returns to landing page | MET | `Header.tsx:32-34,47-49` — `Menu`/`MenuItem` wired to `auth.signoutRedirect()`; `post_logout_redirect_uri` in `auth.ts:12` points to `window.location.origin` (landing page) |
| Hard-refreshing keeps the user signed in | MET | `auth.ts:16` — `userStore: new WebStorageStateStore({ store: window.localStorage })` persists session |
| Session remains valid beyond initial token lifetime | MET | `auth.ts:15` — `automaticSilentRenew: true` |
| `make web.build` and `make web.lint` both pass | MET | Confirmed above — ESLint, `tsc -b`, Prettier, and Vite build all exit clean |

---

## Scope

The diff matches the plan's "Files to Create / Modify" table exactly.

**New files created:** `auth.ts`, `CallbackPage.tsx`, `AuthenticatedPage.tsx` — all present as specified.

**Modified files:** `package.json`, `package-lock.json`, `main.tsx`, `App.tsx`, `LandingPage.tsx`, `Header.tsx` — all match the plan.

**Files outside scope:** `.claude/specs/web-ui/plan.md` is updated to mark the browser sign-in slice as complete. This is a workflow artefact, not feature code, and requires no review.

**No unexplained deviations.** The learnings note that Prettier collapsed one conditional expression in `AuthenticatedPage.tsx` from multi-line to inline — the formatted file matches good Prettier output and passes the format check.

---

## Blockers

None.

---

## Suggestions

#### S1 — `onSigninCallback` duplicated between `auth.ts` and the plan description

- **File:** `src/Worms.Hub.Web/src/auth.ts:17-19`
- **Issue:** The `onSigninCallback` in `auth.ts` only clears the query string; it does not navigate to `/authenticated`. Navigation is handled separately in `CallbackPage.tsx` by watching `auth.isAuthenticated`. The plan's §3 comment says navigation is handled inside `CallbackPage` — which is correct — but keeping both concerns cleanly separated means `auth.ts` stays a pure config file and `CallbackPage` owns the routing decision. The current split is actually well-structured; no change is needed.
- **Fix:** No action required — noting it explicitly so the split is documented.
- **Decision:** Decline

#### S2 — `gatewayUrl` hardcoded to `localhost:5005` in `AuthenticatedPage.tsx`

- **File:** `src/Worms.Hub.Web/src/pages/AuthenticatedPage.tsx:7`
- **Issue:** The Gateway URL is a module-level constant, which is appropriate for a temporary page per the spec. However, as the app grows, each page that calls the Gateway will need this base URL. Extracting it to `auth.ts` (or a sibling `api.ts`) before the next slice is implemented would avoid scattering the port number across multiple files.
- **Fix:** When implementing the next Gateway-calling page, move `gatewayUrl` to a shared constant (e.g. `src/api.ts`). Not a concern for this slice.
- **Decision:** Accept

---

## Nitpicks

#### N1 — `CircularProgress` shown when `auth.user` is present but `access_token` is null

- **File:** `src/Worms.Hub.Web/src/pages/AuthenticatedPage.tsx:32`
- **Issue:** The spinner renders while both `games` and `error` are `null`. If `auth.user?.access_token` is absent (e.g., the token is somehow not yet available even though `isAuthenticated` is true), the effect's early-return means no fetch is issued and the spinner spins indefinitely. This edge case is not in scope per the spec's out-of-scope list (error UI for failed sign-in), and is extremely unlikely in normal operation.
- **Fix:** No action required for this slice. Worth addressing when the temporary page is replaced.
- **Decision:** Decline

---

## Tests

No tests were added or modified. This is consistent with the testing strategy: there are no `.NET` project changes, and there is no guidance in the repo requiring React component tests. The behaviour is exercised manually per the plan's verification steps (§§3–10). The testing strategy document does not establish a web component test tier, so this absence is not a gap.

---

## Recommended Actions

- **S1** — Decline — the concern is already resolved by the existing design; the note is informational only.
- **S2** — Decline — out of scope for this slice; a reasonable future-slice action when the temporary page is replaced.
- **N1** — Decline — explicitly out of scope per the spec; the temporary page will be replaced.
