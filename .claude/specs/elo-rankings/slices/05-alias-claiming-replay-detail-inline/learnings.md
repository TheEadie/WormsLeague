# Learnings: Alias Claiming — Replay Detail Inline

## Implementation Notes

### `npm ci` required before `make web.lint` can pass

The plan instructs running `npx prettier --write src` and then `make web.lint`, but does not mention that `make web.lint` depends on `node_modules` being present. Without a prior `npm ci` (or `make web.build`), ESLint exits immediately with `ERR_MODULE_NOT_FOUND` for `@eslint/js`. Running `npm ci` inside `src/Worms.Hub.Web/` before `make web.lint` resolves this. The web component doc already notes this ordering requirement for CI (`make web.build` must precede `make web.lint`) but the plan did not make this explicit for the local verification steps.

### No other deviations

All changes went exactly as the plan described. The `Button` import, `TeamDto` interface, state variables, `useEffect`, `handleClaim`, `unclaimedTeamsByKey`, and pill augmentation all matched the plan's code snippets without modification. Prettier reported the file as unchanged (no formatting drift from the edits).

## Files Added (not in plan)

None — only `src/Worms.Hub.Web/src/pages/GameDetailPage.tsx` was modified, which the plan listed as the sole changed file.
