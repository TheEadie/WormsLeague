# Learnings: Browser Sign-In

## Implementation Notes

### npm install resolved to newer patch versions

The plan specified `oidc-client-ts@^3.5.0` and `react-oidc-context@^3.3.1`. npm resolved these to the latest patche(s) available and added four packages total (both libraries plus two transitive deps). No warnings or peer dependency conflicts were raised.

### Prettier reformatted AuthenticatedPage.tsx

The `{error !== null && (<Typography>...</Typography>)}` conditional block was collapsed from multi-line to a single inline expression. The plan's ordering (run Prettier before `make web.lint`) handled this cleanly — the lint check then passed without manual intervention.

### Everything else proceeded exactly as planned

- `auth.ts`, `CallbackPage.tsx`, `AuthenticatedPage.tsx` created without deviation.
- `main.tsx`, `App.tsx`, `LandingPage.tsx`, `Header.tsx` updated exactly as described.
- `make web.build` and `make web.lint` both passed on the first attempt with no TypeScript or ESLint errors.

## Files Added (not in plan)

None — all created files match the plan exactly.
