# Learnings: Mockup Alignment

## Implementation Notes

### Prettier reformatted the `<Paper>` element in LandingPage.tsx

As the plan's caveat anticipated, Prettier's `--write` pass reformatted one element: the `<Paper elevation={4} sx={{ ... }}>` opener in `LandingPage.tsx` was collapsed from a multi-line prop layout onto a single line. The plan's ordering (run `npx prettier --write src` before `make web.lint`) handled this cleanly — no manual intervention needed and `prettier --check` then passed.

Everything else proceeded exactly as the plan described:
- `npm install @fontsource/jetbrains-mono@^5.2.5` added the package and regenerated the lockfile without warnings. npm resolved the caret to the current `5.2.8` patch, so `package.json` ended up with `^5.2.8` and the lockfile pinned `5.2.8` — functionally compatible with the plan, just a newer patch than the literal version specified.
- `make web.build` produced the bundle with JetBrains Mono `.woff2`/`.woff` assets emitted to `.artifacts/web/assets/` (latin, latin-ext, cyrillic, greek, vietnamese — all three weights).
- `make web.lint` passed ESLint, `tsc --noEmit`, and Prettier.

## Files Added (not in plan)

None — all created files match the plan exactly.
